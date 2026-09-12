# Chain Composition — FHS Architecture (v1)

> Status: **v1 built and running (2026-08-29).** FHS is a proof of concept whose primary purpose is to find
> out whether this technique holds up under real features. v1 deliberately builds the smallest kernel that
> can answer that question — roughly **half** the surface of the original design.
>
> The kernel, the foundation, seventeen endpoints across all five domain concepts, seven architecture
> tests and a 65-test integration suite on Testcontainers Postgres exist, and the API has been run end to
> end against Postgres and Keycloak under Aspire. **All four of §12's load-bearing assumptions are
> settled** — the contravariant conversions the whole global-link idea rests on do work, at compile time
> and at run time.
>
> **This document is reconciled against the code that was actually written**, and the code has won every
> disagreement. It departs from the original v1 proposal in eight places: two chain kinds instead of one
> (§7), a runner that rethrows instead of swallowing (§6), no scope creation in the runner (§6), shape
> validation as the first *link* rather than an endpoint filter (Rule 7), a command/query `DbContext` split
> (§6), no-tracking reads with explicit writes (Rule 14), optimistic concurrency on every mutable entity
> (§6), and flat State types with `IEndpoint` registration instead of a nested feature class (Rule 12, §9).
>
> **This is the only architecture document.** The longer companion that carried the full original design
> was deleted on 2026-09-01 — it described a v2 that does not exist, and keeping a second document
> reconciled against a moving codebase cost more than it returned. Nothing load-bearing was lost: §14 still
> lists every deferral and the reason for it, which was the only part being read. The successor, when v1 is
> done, is a new document written incrementally against this one — not that file restored.
>
> §13 held the criteria for keeping or abandoning the approach, to be reviewed once roughly ten endpoints
> existed. **That review ran on 2026-09-03 and the verdict is keep** — the measured rows, the three
> decisions it produced and the one row worth worrying about are in §13.

---

## 1. Project context

**FHS (Fault Handling System)** logs and tracks quality faults from a production plant:

| Concept | Meaning |
| --- | --- |
| **Defect** | A fault caught internally, before the product leaves the plant |
| **Escape** | A fault that reached the customer — an internal control failed |
| **Station** | A position on the production line where a defect is detected or caused |
| **Error Code** | The catalogued fault taxonomy that defects are classified against |
| **Customer** | The recipient an escape is reported by or against |

**All five are implemented as of 2026-09-05.** Escape and Customer were the last two, and until that date
they existed only in this table — the system was called a Fault Handling System while handling half the
faults. An Escape carries a customer, a classification against the shared error-code taxonomy, and its own
resolution; it deliberately holds **no link back to a Station or a Defect**, because the inward trace would
be unreliable often enough to mislead. That is a domain decision, not a modelling shortcut: origin analysis
lives outside this system until something can populate it honestly.

### Shape

A **single .NET 10 API plus a SPA frontend, orchestrated by Aspire**. This is a deliberate simplification
of an earlier microservices design: one deployable API, one database, no message broker until something
actually needs one.

```text
infra/FHS.AppHost           Aspire orchestration (Aspire 13.5.x)
infra/FHS.ServiceDefaults   telemetry, health checks, resilience defaults
backend/FHS.Api             vertical slices + shared links + capabilities
backend/FHS.Chain           the composition kernel (no application dependencies, ~275 LOC)
frontend/                   React 19 + TypeScript SPA on Vite (screens: docs/wireframes/README.md)
```

### Decided

- .NET 10 (SDK 10.0.400), Aspire for orchestration, PostgreSQL for persistence.
- Vertical Slice Architecture, with **Chain Composition** (this document) as the convention inside a slice.
- Minimal APIs. No MediatR — see §3.
- **Frontend: React 19 and TypeScript on Vite**, settled 2026-09-12 when the SPA shipped. react-router v8 in
  data mode, axios behind hand-written verb hooks, `react-oidc-context` over Keycloak, Tailwind v4 with
  `light-dark()` tokens, Bun as the package manager. No component kit and no codegen: the earlier
  recommendation of shadcn/ui was not taken, and a generated API client was rejected because the generated
  files conflict in git. Screen-level decisions live in `docs/wireframes/README.md`. This document stays
  backend-only and does not depend on any of it.

### Still open

- Local Kubernetes hosting approach. Currently Aspire on Docker Desktop; decide when it is actually reached.

### Working agreement

**The project code is written by hand, by the developer.** An AI assistant working on this repository
**does not create or edit files under `backend/`, `infra/`, `frontend/`, or the root build files**
(`Directory.Packages.props`, `*.csproj`, `global.json`, `FHS.slnx`). It delivers work as an **ordered list
of steps with the code snippet for each**, naming the file each snippet belongs in, and waits for the step
to be reported done.

`docs/**` is the exception and may be edited directly.

**Why:** the developer stays fluent in the codebase by typing it. This pairs with the build rule below —
**the project is not run incrementally.** Every step is written first and the whole thing is run once at
the end, which is also why **no package, config setting or wiring is added early "to prove" a step
works.** Each dependency arrives in the step that writes the code consuming it, never before. That is the
same instinct as Rule 9 in §8, applied to the build process instead of the code.

---

## 2. The problem this solves

A feature's logic normally ends up distributed across a controller, a handler, a service, a couple of
domain helpers and a repository. Understanding what `POST /defects` does means opening seven files and
holding a call stack in your head. That cost is paid by every developer, every time, forever — and it
grows fastest exactly when a team is onboarding new people.

The goal is a much narrower claim:

> **Reading the declaration at the endpoint should tell you what the feature does, in order, without
> opening another file.**

Chain Composition delivers that by having each feature declare an ordered list of named units. Some units
are shared across features; some belong to that feature alone.

```csharp
private static readonly Chain<CreateDefectState, CreateDefectResponse> Handle = ChainFactory
    .For<CreateDefectState, CreateDefectResponse>()
    .Link<ValidateRequest<CreateDefectRequest>>()  // shared
    .Link<ResolveActor>()                          // shared
    .Link<LoadAndEnsureStationExistence>()         // this feature
    .Link<ClassifyDefect>()                        // this feature
    .Link<RaiseDefect>()                           // this feature
    .Link<RecordDomainEvents>()                    // shared
    .Link<SaveChanges>()                           // shared — commit boundary
    .Build();
```

That is the whole pitch. Everything else in this document exists to stop that list from degrading.

### The trade, stated precisely

The obvious objection to this design is that it hides data flow: in a conventional handler you can see
`station` came from `LoadStation(id)` because it is a parameter, whereas here `LoadStation` writes
`state.Station` and `ClassifyDefect` reads it with nothing at the call site saying so.

Most of that objection does not survive contact with a real codebase, for two reasons:

- **Layered code only appears to document data flow.** `Request → Command → Entity → Dto → Response` means
  the thing you are tracing changes identity at every hop. You are not following data, you are following
  four different shapes of it, and the mapping code between them is where meaning quietly goes missing.
  One `State` per slice removes most of that mapping outright.
- **A link's signature is a compiler-enforced upper bound on what it can touch.** `ILink<IHasActor>`
  *cannot* reach anything but `Actor`. A private method three levels down has no such bound — its
  parameters describe its local surface while it also reaches the `DbContext`, a cache and a static. For
  global links the signature is a stronger guarantee than the nested equivalent, not a weaker one.

**What genuinely survives** is narrow, and worth understanding before reading how it is solved: **the type
system alone cannot check ordering between local links.** Reorder these two and the build fails —

```csharp
var station = await LoadStation(code);          // swap these and you get CS0841:
var classification = Classify(station, code);   // cannot use local 'station' before it is declared
```

— while reordering these two compiles cleanly and leaves `state.Station` null at runtime:

```csharp
.Link<LoadStation>()
.Link<ClassifyDefect>()
```

The mechanism is one sentence: **locals get definite-assignment analysis, fields do not.** `station` is a
local, so the compiler tracks assignment on every path (`CS0841`, `CS0165`). `state.Station` is a field,
and C# performs no such analysis on fields.

**This is closed, not merely mitigated — and it is the cheapest part of the kernel.** Links declare which
State fields they consume and produce (§5), and `Build()` verifies the declared order against those
declarations at the point the chain is written (§5). Reorder `LoadStation` after `ClassifyDefect` and
startup fails with:

```text
ChainWiringException: CreateDefect: 'ClassifyDefect' requires 'Station',
which no earlier link produces.
```

Fifteen lines of set arithmetic, no catalogue, no startup hook, no dedicated test. Two supporting notes:

- **Nullable reference types are still required.** With `Station? Station { get; set; }`, every read site
  raises `CS8602`. **`<Nullable>enable</Nullable>` with `CS8602` escalated to an error is required in this
  project** — already configured in `Directory.Build.props`. The wiring check verifies order; NRT verifies
  that each individual read was thought about.
- **The residual is metadata drift** — a link could declare `[Produces(Station)]` and not assign it. That
  is a much smaller assumption than unverified ordering. Closing it needs a Roslyn analyzer, which is
  **explicitly out of scope for v1** (§14).

So the design does not trade enforcement for visibility. It gets both, at the cost of one or two
attributes on the links that participate in a hand-off.

---

## 3. What this is, and what it is not

**It is** a feature-scoped application of Pipes-and-Filters with Railway-Oriented error handling. That
combination has plenty of precedent — Elixir's `Plug`, Ruby's `inter-actor` gem, Spring Batch — so the
failure modes are known and documented here rather than discovered later.

**It is not:**

| Not this | Because |
| --- | --- |
| A CI/CD pipeline | Unrelated. This is why the words *pipeline*, *stage* and *job* are banned in §4. |
| A workflow engine | **No durability, no persistence, no resume-after-crash, no compensation.** A chain lives and dies inside one HTTP request. If a feature needs durable orchestration it needs Temporal or Elsa, not this. |
| MediatR pipeline behaviors | Those are cross-cutting only and identical for every request. The interesting half here is the *per-feature* sequence, which behaviors cannot express. |
| A new architecture | It is still Vertical Slice Architecture. Chain Composition is a convention *inside* a slice. |

**No MediatR.** Its dispatch indirection works directly against this design's goal — you would trade a
readable declaration list for a `Send()` call that jumps somewhere unnamed. It also went commercial in 2025.

---

## 4. Vocabulary

| Term | Meaning |
| --- | --- |
| **Chain** | The ordered list of links a feature declares. One per feature. |
| **Link** | One unit of work in that list. |
| **State** | The typed object carried through the chain, holding the request and everything links produce. |
| **Capability** | A narrow interface a State implements so shared links can target it. |
| **Local link** | Written against the concrete `State` of one feature. |
| **Global link** | Written against a capability interface, usable by any feature that implements it. |

### Banned words

Names were selected against real collisions in this codebase, and these are off-limits:

`Pipeline`, `Stage`, `Job`, `Step` (CI/CD) · `Path`, `Program`, `Task`, `Activity`, `Filter`, `Handler`,
`Assembly`, `Channel` (BCL) · `Plan`, `Sequence` (PostgreSQL) · `Routing`, `Operation`, `Line`,
`Station`, `Process` (MES terminology and the FHS domain itself) · `Runbook` (incident management —
far too close to a fault-handling domain to reuse).

### Naming convention

**Link classes are bare verb phrases with no suffix**: `ResolveActor`, `LoadStation`, `RaiseDefect`,
`RecordDomainEvents`. Not `LoadStationLink`, not `LoadStationHandler`. This is what makes the
declaration list read as prose — the suffix would appear on every line and carry no information.

---

## 5. Core design decision: typed State with capability interfaces

This is the decision the whole approach lives or dies by.

Almost every homegrown version of this pattern degrades into a context bag —
`context.Items["station"]` with casts and runtime "missing key" failures. That is strictly worse than the
nested calls it replaced. The fix is a **contravariant link interface** plus **capability interfaces**:

```csharp
public interface ILink<in TState>                          // ← note: contravariant
{
    ValueTask<LinkResult> RunAsync(TState state, CancellationToken ct);
}
```

Because `TState` is contravariant, a link written against a capability *is* a link over any State
implementing that capability. Three consequences:

**1. Global links are written once, against only what they need.**

```csharp
[Produces(nameof(IHasActor.Actor))]
public sealed class ResolveActor(ICurrentUser user, IActorDirectory directory) : ILink<IHasActor>
{
    public async ValueTask<LinkResult> RunAsync(IHasActor state, CancellationToken ct)
    {
        if (user.SubjectId is not { Length: > 0 } subjectId)
            return LinkResult.Fail(Errors.Unauthenticated());

        var actor = await directory.FindAsync(subjectId, ct);
        if (actor is null) return LinkResult.Fail(Errors.UnknownActor(subjectId));

        state.Actor = actor;
        return LinkResult.Continue;
    }
}
```

No generics, no dictionary, no casting, and the signature declares its entire surface area: this link
touches `Actor` and nothing else.

**2. Local and global links coexist in one declaration list.**

```csharp
public sealed class LoadStation(FhsCommandDbContext db) : ILink<CreateDefectState>  // local: sees everything
public sealed class ResolveActor(...)                   : ILink<IHasActor>          // global: one field
```

Both satisfy `where TLink : ILink<TState>` when the builder is over `CreateDefectState`. The endpoint
declaration reads uniformly; the coupling difference is visible only in each link's own signature, which
is exactly where it belongs.

**3. The compiler enforces it.** Declaring a global link whose capability the State does not implement is
a *build error*, not a runtime surprise. The entire class of "missing context key" bug does not exist.

### Capability interfaces

Keep them narrow and few. v1 seed set — **three**, promoted only on second use (Rule 9):

```csharp
public interface IHasRequest<out TRequest> { TRequest Request { get; } }
public interface IHasActor                 { Actor Actor { get; set; } }
public interface IRaisesEvents             { List<IDomainEvent> Events { get; } }
```

**A link has exactly one `TState` type parameter.** So a global link needing two capabilities must target
a composite interface that the State declares *explicitly* — C# interface implementation is nominal, so
implementing both parents is not enough:

```csharp
public interface IAuditedWrite : IHasActor, IRaisesEvents;

public sealed class State : ChainState, IAuditedWrite { ... }
//                                      ^^^^^^^^^^^^^ must be listed by name
```

Treat that tax as a feature, not a wart. It is the forcing function behind Rule 8: if a link wants three
capabilities, it probably wants constructor injection instead.

### Links declare their hand-offs

A global link's surface is declared by its type: `ILink<IHasActor>` says "I touch `Actor` and nothing
else," and the compiler holds it to that. A **local** link has no such declaration — `ILink<State>` says
only "I can see everything," which is why ordering between local links is the one thing the type system
cannot check (§2).

So links state it directly, with `nameof`:

```csharp
[Produces(nameof(CreateDefectState.Station))]
public sealed class LoadAndEnsureStationExistence(FhsCommandDbContext db) : ILink<CreateDefectState>

[Requires(nameof(CreateDefectState.Station))]
[Produces(nameof(CreateDefectState.Classification))]
public sealed class ClassifyDefect(FhsCommandDbContext db) : ILink<CreateDefectState>
```

**Global links use the same attributes**, naming the property on their capability interface:
`[Produces(nameof(IHasActor.Actor))]`. Since `nameof(IHasActor.Actor)` and `nameof(State.Actor)` are both
the string `"Actor"`, the two match without the check knowing anything about interfaces. This is a v1
simplification: the full design derived a global link's declarations from its capability interface by
reflection, which is more machinery for the same result on about three shared links.

**`nameof`, not marker interfaces.** `IRequires<IHasStation>` would force a capability interface per
field, exploding the count that Rule 8 exists to keep small. `nameof` gives refactor-safety and
compile-checked existence without inventing types.

**Only declare what crosses a link boundary.** A link reading `state.Request` — always present, set by the
constructor — declares nothing. The attributes describe hand-offs between links, and nothing else. In
`CreateDefect` below, seven links carry four attributes between them.

### The wiring check

It runs inside `Build()`, over the links just accumulated. That placement is what removes the chain
catalogue, the startup hook and the dedicated test from the original design — a chain is a static field
initializer (Rule 13), so the check fires when `Map()` first touches `Handle`, which is at startup.

```csharp
var produced = new HashSet<string>(StringComparer.Ordinal);

foreach (var link in links)
{
    foreach (var required in link.Requires)
        if (!produced.Contains(required))
            throw new ChainWiringException(
                $"{name}: '{link.Name}' requires '{required}', which no earlier link produces.");

    foreach (var field in link.Produces)
        if (!produced.Add(field))
            throw new ChainWiringException(
                $"{name}: '{link.Name}' produces '{field}', which an earlier link already produces.");
}
```

`name` comes from `ChainWiring.NameOf(typeof(TState))`, which takes `DeclaringType?.Name` when the State is
nested and otherwise strips a trailing `"State"` from the type name. Both routes yield `"CreateDefect"`, so
the flat naming the code settled on (Rule 12) costs nothing here.

Two things fall out of that loop:

- **Mutual dependencies are caught.** If two links each require what the other produces, whichever runs
  first fails on an un-produced requirement.
- **Rule 4 is enforced** — two links producing the same field in one chain is caught in the same pass.

One wart, stated plainly: because the throw happens in a static field initializer, .NET wraps it in a
`TypeInitializationException`. The `ChainWiringException` and its message are the `InnerException`. Ugly
first line, correct information one level down, and it happens at startup rather than in production.

Note what is *not* required to make this work: no Roslyn analyzer, no source generation, no reflection over
method bodies, no chain registry. Just the declarations and fifteen lines in the builder.

---

## 6. Execution model: a flat loop

### Links run in a flat loop

The runner iterates an array. It does **not** build a nested delegate chain the way ASP.NET middleware
does. That choice is about one thing: when something throws in production at 3am, the stack trace should
be readable rather than forty frames of `<RunAsync>d__7.MoveNext()`.

```csharp
private async Task<Result> RunCoreAsync<TState>(Chain<TState> chain, TState state, CancellationToken ct)
    where TState : ChainState
{
    using var chainActivity = Activities.StartActivity($"chain {chain.Name}");
    chainActivity?.SetTag("chain.name", chain.Name);

    var current = string.Empty;

    try
    {
        foreach (var descriptor in chain.Links)
        {
            current = descriptor.Name;

            using var activity = Activities.StartActivity($"link {descriptor.Name}");
            activity?.SetTag("chain.name", chain.Name);
            activity?.SetTag("link.name", descriptor.Name);

            var link = (ILink<TState>)services.GetRequiredService(descriptor.Type);
            var outcome = await link.RunAsync(state, ct);

            activity?.SetTag("link.outcome", outcome.Kind.ToString());

            var result = ParseLinkResult(outcome, chainActivity);
            if (result != null) return result.Value;
        }

        chainActivity?.SetTag("chain.outcome", "success");
        return Result.Success;
    }
    catch (Exception ex)
    {
        chainActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        chainActivity?.SetTag("chain.outcome", "threw");
        chainActivity?.SetTag("chain.failed_link", current);
        chainActivity?.AddException(ex);
        throw;
    }
}
```

**That is the whole execution model.** The original design put the exception shield and the parent span
behind an `IChainWrapper` abstraction with an `Order` property and DI registration. There are exactly two
of them, they never vary per feature, and they are five lines each inline — so v1 has no wrapper
abstraction at all. It goes back in when a third around-concern turns up (§14).

Two things in that loop differ from what this document originally proposed, and both are deliberate.

**The runner rethrows; it does not convert exceptions into `Result.Fail(Unexpected)`.** The proposal had
the runner swallow, log and return an error result. Swallowing here is wrong for three reasons: a bug
would be reported to the client as a well-formed `500 ProblemDetails` indistinguishable from a deliberate
`Unavailable`, the exception would be logged by the runner in a format nothing else in the pipeline uses,
and — worst — `OperationCanceledException` from a disconnected client would be recorded as an application
failure. So the `catch` exists only to **annotate the span and let the exception through**: it marks the
activity failed, records which link was running in `chain.failed_link`, and rethrows.

The consequence is a hard requirement on the host: **`FHS.Api` must install an exception handler**
(`AddProblemDetails()` plus `UseExceptionHandler()`), or a thrown link surfaces as an unhandled 500 with a
stack trace in the body. `Error.Unexpected()` survives in the kernel for links that want to *return* an
unexpected-kind failure deliberately; the runner no longer manufactures one. Exceptions are for bugs and
infrastructure faults, and bugs belong to the framework's error pipeline, not to `Result`.

**The runner does not create a DI scope.** The proposal wrapped each run in `services.CreateScope()`. That
is actively harmful in an ASP.NET request: it would give links a *second* `DbContext`, distinct from the
one the request already has, so `SaveChanges` would commit a different change-tracker than the one earlier
links wrote into. The runner resolves from the ambient `IServiceProvider` instead, which is the request
scope.

> **Registration requirement: `ChainRunner` is registered scoped**, and so is every link. Registering the
> runner as a singleton captures the root provider and reintroduces exactly the split-`DbContext` bug this
> avoids. `AddChain` (§7) is what guarantees this, and it is the reason that method is part of
> the kernel rather than a line in `Program.cs`.

### The commit boundary

**EF Core's `DbContext` is already the unit of work**, and `SaveChangesAsync` wraps all tracked changes in
an implicit transaction. An explicit transaction buys nothing for a single-save feature, so there is no
unit-of-work wrapper and no `CommitChanges` link. **The feature's own saving link commits.**

That leaves one real hazard, and it is a correctness bug rather than a matter of taste: if the saving link
sits at position 5 of 7 and link 6 fails, **the write is already committed and the client gets an error
response for an operation that persisted.** Hence Rule 10:

> **Nothing that can fail follows the saving link.**

| Segment | If it fails |
| --- | --- |
| any `.Link<>()` before the save | Nothing persisted, error response |
| the saving `.Link<>()` | Nothing persisted (EF's implicit transaction), error response |
| a post-save `.Link<>()` | **Must have no `Fail` path** — see below |

v1 has **no post-commit segment**, and on the reasoning below it is unlikely to grow one. The original
design's `.OnCommitted<>()` — for notifications, cache invalidation, outbound messages — costs a second
link array, a second loop, a second set of failure semantics and a second entry in `Describe()`. All of
that machinery exists to stop a post-commit failure from reaching the client. **A post-save link that only
*enqueues* cannot fail**, so it needs none of it, and it is an ordinary `.Link<>()` at the end of the
ordinary declaration:

```csharp
.Link<SaveChanges>()             // commit boundary
.Link<InvalidateDefectCache>()   // enqueues only — no Fail path
```

The cost is that Rule 10 stops being checkable from the declaration alone and becomes a property of the
trailing link's body. With one such link that is a review question. If post-save links multiply, that is a
§13 signal and `.OnCommitted<>()` comes back (§14).

### Post-commit work: two tiers

That enqueue hands off to a background service. Which one depends on a single question — **can this be
lost?**

| | In-memory `Channel<T>` → hosted service | Outbox row → background reader |
| --- | --- | --- |
| Durability | Lost on shutdown or crash | Commits in the same transaction as the data |
| Latency | Microseconds — the reader wakes on write | The poll interval — seconds |
| Backstop on loss | The cache TTL | None needed |
| Fits | Cache invalidation | Integration events, outbound messages |

The latency row is why cache invalidation does **not** go through the outbox: a five-second poll is a
five-second window in which a resolved defect still reads as open. The durability row is why nothing that
must survive a restart goes through the channel.

**Scaling out changes the broadcast, not the channel.** The channel is per-instance and stays that way;
what breaks at N instances is that evicting one instance's L1 leaves the other N−1 stale. That is fixed by
a **pub/sub broadcast** — never a work queue, where exactly one consumer would win and the rest would stay
stale, making the problem strictly worse than before scaling out. `HybridCache` may already do this through
its own back-plane, in which case none of it is hand-written. Confirm that before building it.

`SaveChanges` itself is a **shared** link and needs nothing from State at all, because EF is already
tracking what earlier links added or modified:

```csharp
public sealed class SaveChanges(FhsCommandDbContext db) : ILink<ChainState>
{
    public async ValueTask<LinkResult> RunAsync(ChainState state, CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return LinkResult.Fail(Errors.ConcurrencyConflict());
        }

        return LinkResult.Continue;
    }
}
```

Targeting the `ChainState` base means every State satisfies it, so one class serves every feature — the
capability approach at its cleanest, with a state surface of zero.

The `catch` is deliberately narrow. A concurrency conflict is a **business outcome** — someone else changed
the row — so it becomes a `409`, not a 500. `DbUpdateException` (foreign key, length, unique) is a *bug*
that validation or a business-rule link should have caught, so it keeps going to the exception handler.

### Two DbContexts

Persistence is split: **`FhsCommandDbContext` for writes, `FhsQueryDbContext` (no change tracking) for
plain query endpoints.** The boundary is the same one Rule 1 already draws — chains are the command side,
plain handlers are the query side:

> **Links always take `FhsCommandDbContext`. `FhsQueryDbContext` is for plain GET endpoints only.**

The two are separate connections, so nothing inside one chain may mix them. Loading an entity through the
query context and mutating it in a later link would leave `SaveChanges` on the command context with
nothing to write, and the update would vanish with no error.

Only the command context owns migrations. The query context has no design-time factory, which makes it
un-migratable by construction — both map the same entities, so a second migration history would collide.
Health checks follow **connection strings, not contexts**: one shared string means one check today, and two
the day the query context points at a read replica.

This is also why the shared link is named `RecordDomainEvents` and not `DispatchDomainEvents`. It records
events as outbox rows *before* the save, so they commit atomically with the data; actual dispatch happens
after commit, by a background reader. The name makes the required position obvious.

### Results, not exceptions

```csharp
public enum ErrorKind { Validation, NotFound, Conflict, Forbidden, Unavailable, Unexpected }

public sealed record FieldError(string Field, string Code, string Message);

public sealed record Error(
    string Code,
    string Message,
    ErrorKind Kind,
    IReadOnlyList<FieldError>? Fields = null);       // multiple failures reported together

public readonly record struct LinkResult
{
    public static LinkResult Continue { get; }        // run the next link
    public static LinkResult Done     { get; }        // stop early, successfully
    public static LinkResult Fail(Error error);       // stop, map to a problem response
}
```

`Done` matters: a cache hit or an idempotent replay must be able to stop the chain *successfully* — and
because the endpoint reads its result off `State` (§7), a short-circuiting link can write that field and
return `Done` with no special support from the kernel.

`ErrorKind` is mapped to an HTTP status in exactly one place — an `Error.ToProblem()` extension — and
surfaced as `ProblemDetails`. Exceptions are for bugs and infrastructure faults only, never for expected
business outcomes.

`ToProblem()` lives in `FHS.Api/Extensions/ErrorExtensions.cs` — the kernel has no ASP.NET dependency and
does not know what an HTTP status code is.

**There is both a `Result` and a `Result<T>`.** See §7.

---

## 7. Two chain kinds, one runner core

This document originally specified **one** chain kind. `Chain<TState>` would serve every feature, and a
value-producing feature would let its endpoint read the value straight off the `State` it had just
constructed. The argument for cutting the second kind was cost: two state bases, two chain types, two
builders, two runner overloads, `Result` and `Result<T>`, two `MapChain` overloads — **six parallel
pairs, for one behavior.**

**The kernel that was built has both kinds, and the cost estimate turned out to be wrong.** The pairs are
not parallel; they are related by inheritance, so the second kind is additive rather than duplicative:

```csharp
public abstract class ChainState;                          // empty — a constraint anchor

public abstract class ChainState<TResult> : ChainState     // adds Produce()
{
    public void Produce(TResult result);
    internal bool HasResult { get; }
    internal TResult Result { get; }                       // throws ChainResultException if !HasResult
}

public class Chain<TState> where TState : ChainState
{
    public string Name { get; }
    public IReadOnlyList<LinkDescriptor> Links { get; }
    public IReadOnlyList<string> Describe();               // ordered link names
}

public sealed class Chain<TState, TResult> : Chain<TState> // adds nothing but a type argument
    where TState : ChainState<TResult>;
```

Because `Chain<TState, TResult>` **is a** `Chain<TState>`, the runner needs one loop, not two:

```csharp
public Task<Result> RunAsync<TState>(Chain<TState> chain, TState state, CancellationToken ct)
    where TState : ChainState => RunCoreAsync(chain, state, ct);

public async Task<Result<TResult>> RunAsync<TState, TResult>(
    Chain<TState, TResult> chain, TState state, CancellationToken ct)
    where TState : ChainState<TResult>
{
    var result = await RunCoreAsync(chain, state, ct);

    return result.IsSuccess
        ? Result<TResult>.Success(state.Result)
        : Result<TResult>.Fail(result.Error);
}
```

Six lines. `ChainWiring.Verify` is shared by both builders, `LinkDescriptor` is shared, `ILink<in TState>`
is untouched. **The real duplication is one 26-line builder** — `ChainBuilder<TState, TResult>`, which
differs from `ChainBuilder<TState>` only in what `Build()` returns — and there is no `MapChain` to
double (below). That is a long way from six parallel pairs, and it buys three things the one-kind design
gave up:

- **"Finished without producing" is named.** The one-kind design relied on `Defect` being nullable, so the
  endpoint's read raised `CS8602`, and an `!` past it produced a bare `NullReferenceException`. Reading
  `state.Result` without a preceding `Produce()` instead throws `ChainResultException` with a message that
  names the chain and lists both causes — no link produced, or a link returned `Done` before the producing
  one ran. That is the diagnostic the deferred `Produced<T>` struct (§14) was invented to supply, obtained
  here for free.
- **The endpoint stops carrying `!`.** `result.ToCreated(...)` reads better than `state.Defect!.Id`, and
  more importantly the endpoint no longer needs to know *which State field* was the point of the feature.
- **The result mapping generalises.** `Result<T>` is what `FHS.Api/Extensions/ResultExtensions.cs` hangs
  `Match`, `ToOk`, `ToNoContent` and `ToCreated` off. With only `Result`, each endpoint would rewrite that
  ternary by hand.

**`ILink<in TState>` is still the single link contract**, which was always the real reason not to let links
return `TResult`. `ResolveActor`, `RecordDomainEvents` and `SaveChanges` are shared across both kinds
unmodified, because a `ChainState<TResult>` *is* a `ChainState`. Early `Done` still yields a value — the
short-circuiting link calls `Produce()` and returns `Done`.

**Use `Chain<TState>` when the feature genuinely returns nothing** (`204 No Content`). Do not invent a
`TResult` to have one; the point of keeping both kinds is that each feature says which it is.

`ChainState` stays as an empty base purely so `SaveChanges : ILink<ChainState>` has something to target.

### No `MapChain`

The original wrapped endpoint registration in a generic `MapChain(route, chain, stateFactory, resultMapper)`
helper. Its overload set has to cover route shape × parameter binding × result mapping, and §9's two
example features already needed two different signatures. v1 writes the endpoint as a plain minimal API
delegate — six lines, binds anything, zero kernel surface (§9).

**This is now a decision rather than a deferral** (2026-09-03, §13 review). With ten endpoints written the
route shapes are known, which was the stated trigger — and the answer is no. A `MapChain` helper would
absorb the `runner.RunAsync(Handle, …)` line, and that line is where the endpoint file stops being a route
registration and starts being the feature. §2's claim is that the declaration at the endpoint tells you
what the feature does; hiding the invocation behind a helper moves the reader one indirection further from
the `.Link<>()` list that is the entire point. The six lines are the deliverable, not the overhead.

The *result mapping* third of that helper already exists without the other two, as extension members on
`Result<T>` in `FHS.Api/Extensions/ResultExtensions.cs` — `Match`, `ToOk`, `ToNoContent`, `ToCreated`.
They compose with a plain `MapPost`, so the endpoint stays a normal minimal API delegate and still gets
one-line result handling. The non-generic `Result` carries its own `Match` and `ToNoContent`, which is what
`Chain<TState>` features return through — an earlier gap here, now closed.

### Kernel surface

`FHS.Chain`, no application dependencies. **As built: 275 significant lines across 21 files** (411
including blanks and usings), against a stated target of ≈230. The overrun is the second chain kind and
the richer span tagging, and it is accepted. **The cap is now ~300.** If it grows past that, the pattern
is being pushed past its fit — see the kernel-size row in §13.

| Type | File | Role | LOC |
| --- | --- | --- | --- |
| `ILink<in TState>` | `Contracts/ILink\`.cs` | The one unit contract | 8 |
| `ChainState`, `ChainState<TResult>` | `Contracts/ChainState*.cs` | Constraint anchor; `Produce()` and the guarded `Result` | 33 |
| `LinkResult`, `LinkOutcome` | `Primitives/`, `Enums/` | `Continue` / `Done` / `Fail(Error)` | 30 |
| `Chain<TState>`, `Chain<TState, TResult>` | `Chain\`.cs` | Immutable descriptor array, `Name`, `Describe()` | 25 |
| `LinkDescriptor` | `LinkDescriptor.cs` | `Type`, `Name`, `Requires[]`, `Produces[]` | 28 |
| `ChainFactory.For<…>()` | `ChainFactory.cs` | Builder entry point for both kinds | 13 |
| `ChainBuilder<TState>`, `ChainBuilder<TState, TResult>` | `Builder/` | `.Link<T>()`, `.Build()` | 52 |
| `ChainWiring` | `ChainWiring.cs` | The order check and the chain's name; shared by both builders | 32 |
| `ChainRunner` | `ChainRunner.cs` | The flat loop, span tagging, the two `RunAsync` overloads | 88 |
| `RequiresAttribute`, `ProducesAttribute` | `Attributes/` | The hand-off declarations | 14 |
| `ChainWiringException`, `ChainResultException` | `Exceptions/` | Their two failure modes | 6 |
| `Result`, `Result<T>`, `Error`, `FieldError`, `ErrorKind` | `Primitives/`, `Enums/` | Hand-rolled, no dependency | 82 |
| `AddChain(assembly)` | `ChainRegistration.cs` | Scan `ILink<>` implementations, register them and the runner **scoped** | 28 |

`LinkDescriptor` carries `Type`, `Name`, `Requires[]`, `Produces[]` — the attributes are read once, by
reflection, inside `.Link<T>()`, which runs at static-field-initializer time, not per request. Its `Name`
renders a generic link as `ValidateRequest<CreateDefectRequest>` rather than the raw ``ValidateRequest`1``,
so spans and `Describe()` stay readable.

`AddChain` is not optional bookkeeping: it is what enforces the scoped-lifetime requirement from §6, and
without it nothing resolves `descriptor.Type` at all. **It deliberately skips open generics**, so a generic
link such as `ValidateRequest<>` needs its own `services.AddScoped(typeof(ValidateRequest<>))` — a gap with
no compile-time signal, which is why §11 has a test for it.

---

## 8. Rules and limits

These are the part that actually protects maintainability. The pattern degrades without them.

1. **Every endpoint is a chain, reads included.** This rule said the opposite until 2026-09-01 — that a
   chain had to earn its place with three or more meaningful operations or a shared concern, that a
   `GET /defects/{id}` stays a plain endpoint calling a query, and that a mixed codebase is *correct*
   rather than a failure of discipline. The old text is kept here because the reversal is the interesting
   part, and because the argument for the exemption was a good one.

   What overturned it is that the exemption bought less than it appeared to. `ChainRunner` is the only
   thing that opens the `chain` and `link` spans in §10, so every plain handler is a hole in the one trace
   view — and reads are the endpoints most likely to be the slow ones. `Result<T>` reaching the client
   through `ToOk()` and `ToProblem()` is what stops a read's `404` from drifting away from a write's `404`
   by hand. And a read that later needs the actor — defects filtered to the operator's own line — already
   has somewhere to put the link, where a plain handler would have to be rewritten into a slice first.

   **The cost is real and is not hidden here:** all four read chains are a single link, which Rule 2 as
   written would have forbidden and which §13 counts as ceremony. Rule 2 and the §13 signal table were
   both amended in the same pass rather than left to contradict this.

   The escape hatch at the end of this section still stands. It now has no users.
2. **Three to eight links on a write. One is normal on a read.** More than eight means the feature is
   really two features, or the links have been sliced too thin to carry meaning. Reads sit outside the band
   by construction rather than by exception: a query link builds one query and produces one result, and
   splitting it in two would mean either running the query twice or handing an unexecuted `IQueryable`
   through State to be run somewhere else — which is exactly the action at a distance Rule 14 exists to
   prevent.
3. **Linear only. No branching combinator in v1.** No `when:` guards, no nested chains, no loops, no jumps.
   A branch *tree* means two features and two chains, selected at the endpoint. Linear composition handles
   branching badly — this is its best-known failure mode, and the rule is the mitigation. A link that
   conditionally does nothing (`if (!x) return LinkResult.Continue;`) covers the easy case without kernel
   support. `.Link<T>(when:)` is deferred (§14) — note that a conditional link can never be a guaranteed
   producer, so restoring it means teaching the wiring check about it.
4. **One writer per field, named after it, and declared.** Every State field a chain produces is nullable
   and written by exactly one link, named for what it produces: `LoadStation` → `State.Station`,
   `ClassifyDefect` → `State.Classification`. Links declare hand-offs with `[Requires]`/`[Produces]` (§5).
   **This is what closes the ordering gap in §2** — `Build()` verifies every chain against those
   declarations, and one-writer-per-field is enforced in the same pass. Only declare fields that cross a
   link boundary; `state.Request` is always present and needs nothing.
5. **Links never call other links.** Doing so reinstates exactly the nested tracing this design exists to
   remove. Enforced by an architecture test, not by good intentions.
6. **Links are stateless.** Dependencies through the primary constructor, no mutable fields. They are
   resolved per request but must not rely on it.
7. **Do not absorb framework concerns.** Authentication and authorization → endpoint filters and policies.
   Rate limiting → middleware. Links carry **business** sequence only. Wanting to make everything a link is
   the main way this bloats. Validation is two steps, and the split is by *what the rule needs*:
   - **Shape** ("StationCode is required", "Description ≤ 500 chars"). Runs as the **first link** in the
     chain, `ValidateRequest<TRequest>`, and reports every failure at once.
   - **Business state** ("that station is decommissioned", "that code is already in use"). Needs loaded
     data, so it belongs in a link further down.

   **This document originally put shape validation in an endpoint filter, and the code does not.** The
   reason is the error shape: the kernel already has `Error.Fields`/`FieldError`, and `ToProblem()` already
   emits them as `extensions["errors"]`, but *nothing produced them*. A validation link fills that in, so a
   shape failure and a business failure reach the client as the same `ProblemDetails` shape. A filter would
   have produced a second, different one. The cost is that a rejected request still pays for `State`
   construction and one link resolution, which is small enough to accept.

   This also settles the open question about .NET 10's built-in `AddValidation()`: it is filter-based and
   exposes no `IValidator<T>` to call from a link, so choosing the link means **FluentValidation**.

   `ValidateRequest<TRequest> : ILink<IHasRequest<TRequest>>` is the sole consumer of the `IHasRequest`
   capability, and it is generic — which `AddChain` **will not register**, because the scan skips open
   generics and the closed form never appears in the assembly. It needs an explicit
   `services.AddScoped(typeof(ValidateRequest<>))`. The architecture test in §11 exists partly to catch
   that.

   Keep validators synchronous and pure. A database call inside a validator hides a business rule from the
   chain declaration, which is the entire point of the design.
8. **Capabilities carry what the chain produces; ambient dependencies come from DI.** Clock, current user,
   tenant and configuration are constructor-injected — they are not State. This is what keeps the
   capability set small enough to memorize.
9. **Stay local until a second feature needs it.** Write `ILink<TState>` first. Promote to a
   capability-typed global link on the *second* use, never in anticipation of it. Premature capability
   interfaces are how a shared catalogue turns into a pile of one-user abstractions. **This rule applies to
   the kernel itself** — it is the reason v1 has three capabilities and no wrapper abstraction.
10. **Nothing that can fail follows the saving link.** Anything that can fail after a commit produces the
    worst outcome available: a persisted write and an error response. A post-save link that only *enqueues*
    is therefore permitted — an in-memory `TryWrite` has no failure mode that should reach the client — and
    one that performs the work itself is not. See §6 for the two tiers the enqueue hands off to.
11. **Fold existence and state checks into the link that loads.** `EnsureStationExists` followed by
    `LoadStation` is two round trips to learn one fact — the load *is* the check, returning `Fail(NotFound)`
    when it comes back empty. **The as-built code goes further and folds the state check in too**:
    `LoadAndEnsureStationExistence` returns `404` when the station is missing and `409` when it is
    decommissioned, rather than splitting into `LoadStation` + `EnsureStationActive`. The reason is
    consistency — `ClassifyDefect` had always done load-and-check in one link, and having Station do it in
    two was an inconsistency with no justification.

    **Know what that costs**: "the station must be active" no longer has its own line in the declaration,
    so §2's promise that the list tells you the feature's rules is weaker by one rule. Watch it as a §13
    signal. Checks with *no* downstream product — uniqueness, cross-field rules, quotas — still earn their
    own link, and seeing `EnsureCodeNotDuplicated` in the declaration is exactly the value at stake.
12. **Every feature declares its own State**, and **the base stays free of domain fields.** Every field a
    feature uses is declared, with its real type, in that feature's own file. The moment a shared base
    carries `Station`, the property that makes a slice readable in isolation is gone.

    The original design nested `State` inside a `CreateDefect` class, which is where `ChainWiring.NameOf`
    got the chain's name from (`typeof(TState).DeclaringType`). **The as-built code uses flat type names**
    — `CreateDefectState`, `CreateDefectRequest` — in separate files. `NameOf` therefore falls back to
    stripping a trailing `"State"` from the type name, which yields the same `"CreateDefect"` in spans and
    wiring errors.
13. **Keep the declaration statically analyzable.** A chain is always a static literal fluent chain in a
    field initializer — never built at runtime, never conditionally, never in a loop. And a link never
    passes `state` to another method: read it, write it, do not hand it off. Both are free to follow now,
    expensive to retrofit, and they are what keeps the optional analyzer buildable later (§14).

    **Correction to an earlier claim:** this document said the `Build()` check "fires at startup." With the
    `IEndpoint` pattern it does not. `MapEndpoint` never reads the static `Handle` field — only the request
    delegate does — so under `beforeFieldInit` the type initializer, and therefore the wiring check, runs on
    the **first request to that endpoint**. The architecture test in §11 reads every chain field by
    reflection, which is what turns the check back into a build-time guard.
14. **Reads are `AsNoTracking()`; writes are explicit.** A link that changes something calls `db.Update(…)`
    or `db.Add(…)` by name. Relying on the change tracker to persist a mutation is action at a distance —
    the same "go trace it in another file" cost this design exists to remove — and load-then-map-then-
    `Update` against a tracked instance throws *"another instance with the same key is already being
    tracked"*, which no-tracking reads make unreachable.

    Two consequences. `db.Update` marks **every** property modified, so the statement rewrites the whole
    row; that is safe only because every mutable entity carries an optimistic concurrency token (§6). And
    **`ExecuteUpdate`/`ExecuteDelete` are banned inside a link** — they execute immediately against the
    connection and do not participate in `SaveChanges`, so they would commit outside the boundary, before
    `RecordDomainEvents` has written its outbox rows. That breaks Rule 10 and the outbox's atomicity, and it
    looks like it worked.

**Escape hatch, stated plainly:** if a feature fights the chain, write a plain handler and move on. That
is a permitted outcome, not a defeat. Record it — a growing count of escapes is the most useful signal
this document can produce (§13).

---

## 9. Anatomy of a slice

```text
backend/FHS.Api/Features/Defects/
    DefectErrors.cs                      ← Error factory shared across the Defects area
    CreateDefect/
        Endpoint.cs                      ← the chain declaration and the route
        State.cs
        Models.cs                        ← Request, Response, value objects, domain events
        Validator.cs                     ← FluentValidation rules for the Request
        Links/
            LoadAndEnsureStationExistence.cs
            ClassifyDefect.cs
            RaiseDefect.cs
```

Shared links live in `backend/FHS.Api/Links/`, capabilities in `backend/FHS.Api/Interfaces/`.

**A read slice has the same shape**, since Rule 1 stopped exempting reads — `State.cs`, `Models.cs`, one
link under `Links/`, `Endpoint.cs`, with no `Validator.cs` while the query parameters are clamped rather
than validated. It differs in exactly two ways: the link takes `FhsQueryDbContext` instead of
`FhsCommandDbContext`, and the State carries **no capability interface**. That second one is Rule 9 in
practice — `IHasRequest` exists for `ValidateRequestInput` and nothing else, so a read that runs no
validation link declares a plain `Request` property and adds the interface on the day it needs one.

**Reads are paged only when the collection is unbounded.** `GET /defects` returns
`PagedResponse<DefectListItem>` because the defect table grows without limit; `GET /stations` and
`GET /error-codes` return a bare `IReadOnlyList<T>` because they are reference data, bounded by the
physical plant and the fault taxonomy. This is a deliberate asymmetry, decided on 2026-09-03: those two
endpoints are what the SPA's dropdowns bind to, so wrapping them in a paged envelope would immediately
require a second, unpaged endpoint alongside each — paying for the envelope twice and getting nothing.
The rule for the next read endpoint is that question, not consistency for its own sake: **can this
collection grow without bound? Then page it.** Filtering is orthogonal and both kinds get it.

Endpoints implement `IEndpoint`, a one-member interface with a `static abstract MapEndpoint`, and
`app.MapApiEndpoints()` reflects over the assembly to call each one. That is the *registration* third of the
deferred `MapChain` (§14) — the cheap third — leaving route shape, binding and result mapping as a plain
minimal API delegate, which is why the other two thirds were not worth building.

### Example A — a feature that returns a value

```csharp
// State.cs
public sealed class CreateDefectState(CreateDefectRequest request)
    : ChainState<CreateDefectResponse>,
        IHasRequest<CreateDefectRequest>, IHasActor, IRaisesEvents
{
    public CreateDefectRequest Request { get; } = request;
    public Actor Actor { get; set; } = null!;            // written by ResolveActor
    public List<IDomainEvent> Events { get; } = [];

    public Station? Station { get; set; }                // written by LoadAndEnsureStationExistence
    public Classification? Classification { get; set; }  // written by ClassifyDefect
}

// Endpoint.cs
public sealed class CreateDefectEndpoint : IEndpoint
{
    private static readonly Chain<CreateDefectState, CreateDefectResponse> Handle = ChainFactory
        .For<CreateDefectState, CreateDefectResponse>()
        .Link<ValidateRequest<CreateDefectRequest>>()   // shared  ILink<IHasRequest<T>>  — shape rules
        .Link<ResolveActor>()                           // shared  ILink<IHasActor>
        .Link<LoadAndEnsureStationExistence>()          // local   — 404 missing, 409 decommissioned
        .Link<ClassifyDefect>()                         // local
        .Link<RaiseDefect>()                            // local   — calls state.Produce(...)
        .Link<RecordDomainEvents>()                     // shared  ILink<IRaisesEvents>
        .Link<SaveChanges>()                            // shared  ILink<ChainState> — commit, last
        .Build();

    public static void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/defects", async (
                CreateDefectRequest request, ChainRunner runner, CancellationToken ct) =>
            {
                var result = await runner.RunAsync(Handle, new CreateDefectState(request), ct);
                return result.ToCreated(r => $"/defects/{r.DefectId}");
            })
           .WithName("Create Defect")
           .WithTags("Defects")
           .RequireAuthorization();
}
```

Note that `Defect` is **not** a State field. `RaiseDefect` constructs the entity, adds it to the
`DbContext` and calls `state.Produce(new Response(defect.Id))`; nothing downstream reads the entity, so it
never needs to cross a link boundary. Had it been a field it would have needed `[Produces]`, a nullable
declaration and a `!` at the endpoint — the second chain kind (§7) removes all three. **Only put a value
on `State` when a later link reads it.**

Read the declaration and you have the feature: that the request is well-formed, who the actor is, that the
station must exist and be usable, that the defect gets classified and raised, and where it commits. Note
what is *absent*: no separate FK-existence link (Rule 11 — the load is the check), no transaction ceremony,
and no try/catch.

The endpoint returns `IResult`; tighten to `Results<Created<Response>, ProblemHttpResult>` when OpenAPI
output starts to matter.

A local link, for contrast — narrow, one field, one reason to fail:

```csharp
[Produces(nameof(CreateDefectState.Station))]
public sealed class LoadAndEnsureStationExistence(FhsCommandDbContext db) : ILink<CreateDefectState>
{
    public async ValueTask<LinkResult> RunAsync(CreateDefectState state, CancellationToken ct)
    {
        var code = state.Request.StationCode;

        var station = await db.Set<Station>()
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Code == code, ct);

        if (station is null) return LinkResult.Fail(DefectErrors.StationNotFound(code));
        if (!station.IsActive) return LinkResult.Fail(DefectErrors.StationInactive(code));

        state.Station = station;
        return LinkResult.Continue;
    }
}
```

It reads `state.Request`, which is set by the constructor and always present, so that needs no
declaration. It produces `Station`, which a later link consumes — so that does.

`db.Set<Station>()` rather than `db.Stations`: the contexts declare **no `DbSet` properties**, which makes
`IEntityTypeConfiguration<T>` the single registration path for the model instead of two. The trade is that
the context is no longer a readable inventory of the schema, and that EF names tables after the entity type
rather than a pluralised set name — hence `station`, `defect`, `error_code`, singular and snake_cased.

### Example B — a feature that returns nothing, reusing the same shared links

```csharp
public sealed class ResolveDefectState(Guid defectId, ResolveDefectRequest request)
    : ChainState, IHasRequest<ResolveDefectRequest>, IHasActor, IRaisesEvents
{
    public Guid DefectId { get; } = defectId;
    public ResolveDefectRequest Request { get; } = request;
    public Actor Actor { get; set; } = null!;
    public List<IDomainEvent> Events { get; } = [];

    public Defect? Defect { get; set; }        // written by LoadAndEnsureDefectIsOpen
}

public sealed class ResolveDefectEndpoint : IEndpoint
{
    private static readonly Chain<ResolveDefectState> Handle = ChainFactory
        .For<ResolveDefectState>()
        .Link<ValidateRequest<ResolveDefectRequest>>()  // shared — same generic link, new type argument
        .Link<ResolveActor>()                           // shared — identical class, unmodified
        .Link<LoadAndEnsureDefectIsOpen>()              // local
        .Link<MarkResolved>()                           // local  — calls db.Update(...)
        .Link<RecordDomainEvents>()                     // shared — identical class, unmodified
        .Link<SaveChanges>()                            // shared — identical class, unmodified
        .Build();

    public static void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/defects/{id:guid}/resolve", async (
                Guid id, ResolveDefectRequest request, ChainRunner runner, CancellationToken ct) =>
            {
                var result = await runner.RunAsync(Handle, new ResolveDefectState(id, request), ct);
                return result.ToNoContent();
            })
           .WithName("Resolve Defect")
           .WithTags("Defects")
           .RequireAuthorization();
}
```

`ResolveDefect` genuinely returns nothing, so its State derives from plain `ChainState`, its chain is
`Chain<State>`, and the runner's non-generic overload returns `Result`. `CreateDefect` derives from
`ChainState<Response>` and gets `Result<Response>`. **The declaration lists are otherwise identical in
form**, and the choice between the two kinds is made in exactly one place — the State's base class —
which then propagates through `ChainFactory.For<…>()` by inference.

**This is the claim the POC set out to test, and it held.** `ValidateRequest<T>`, `ResolveActor`,
`RecordDomainEvents` and `SaveChanges` — four of the six links — appear in both chains with no changes at
all, across two different chain kinds. That is the payoff of §5's single link contract.

`POST` rather than `PATCH`, because calling it twice differs from calling it once: the second attempt
returns `409`. That rules out `PUT`'s idempotency promise, and `PATCH` would invite a single fat endpoint
dispatching on which fields arrived — the opposite of one slice per command. **The as-built route is
`/defects/{id}/resolve`**; this section said `/resolution` until 2026-09-05, and the code was right.

Note the **four** levels of coupling visible in these declarations, each chosen by the link's own signature
rather than by where it sits in the list — the fourth appeared with the generic validation link, which did
not exist when this section was first written:

| Link | Contract | Sees |
| --- | --- | --- |
| `SaveChanges` | `ILink<ChainState>` | nothing |
| `ResolveActor` | `ILink<IHasActor>` | one field |
| `ValidateRequest<T>` | `ILink<IHasRequest<T>>` | one field, generically |
| `MarkResolved` | `ILink<ResolveDefectState>` | the whole feature |

### Registration

```csharp
var assemblyReference = typeof(Program).Assembly;

builder.Services
    .AddDbContexts(builder.Configuration, builder.Environment)
    .AddJwtAuthentication(builder.Configuration, builder.Environment)
    .AddValidation(assemblyReference)   // FluentValidation + the open generic ValidateRequest<>
    .AddChain(assemblyReference);       // ILink<> implementations and ChainRunner, all scoped

// ...
app.MapApiEndpoints();                  // reflects over IEndpoint and calls MapEndpoint on each
```

`AddChain` is the kernel's own registration method — the name `AddChainComposition` in earlier drafts was
never built. Note that `AddValidation` must supply `services.AddScoped(typeof(ValidateRequest<>))`
separately, because `AddChain`'s assembly scan skips open generics (Rule 7).

---

## 10. Observability

**One span per link, inside one span per chain.** The source name is `ChainRunner.ActivitySourceName`
(`"FHS.Chain"`).

| Span | Tags |
| --- | --- |
| `chain {Name}` | `chain.name`, `chain.outcome` (`success` / `done` / `fail` / `threw`), `chain.error` on a failure, `chain.failed_link` on a throw |
| `link {Name}` | `chain.name`, `link.name`, `link.outcome` |

`chain.failed_link` is the payoff of the re-throwing runner (§6): the exception keeps its own stack trace
*and* the span records which link was executing when it escaped.

> **This required one line in the API's telemetry setup, and it is there.** An `ActivitySource` nothing
> has subscribed to produces no spans at all: `ConfigureOpenTelemetry` in `FHS.ServiceDefaults` calls
> `tracing.AddSource(builder.Environment.ApplicationName)`, which does **not** match `"FHS.Chain"`.
> `Program.cs` adds `.AddSource(ChainRunner.ActivitySourceName)` separately, which is what keeps this
> section from being silently inert — the worst failure mode a telemetry bug has. Recorded as satisfied on
> 2026-09-05; it had been carried as a standing hazard long after it was closed.

Because FHS runs under Aspire, the dashboard's trace waterfall becomes a **live diagram of the feature**,
showing the real order and the real cost of every link, per request. This is a genuine and somewhat
under-appreciated payoff of combining this pattern with Aspire: the documentation of what happened is
generated by the thing happening. It is also five lines in the runner, which is why it survives into v1
when the wrapper abstraction around it did not.

`GET /_chains` — the JSON dump of every registered chain — is deferred (§14). It needs a chain catalogue,
and the catalogue's other job, feeding the wiring check, moved into `Build()`. `Chain<TState>.Describe()`
stays, so adding the endpoint back is a small job whenever it is wanted.

---

## 11. Testing

**Two levels, not three.** The original design had a link tier under these; it was cut on 2026-09-01 and
the reasoning is below.

| Level | What it covers |
| --- | --- |
| **Integration test** | `WebApplicationFactory` + Testcontainers PostgreSQL, one per endpoint, asserting status codes and persisted state. Built, green, and described below. |
| **Architecture test** | Seven, in `tests/Fhs.ArchitectureTests`. Built, green, and described below. |

The last one was worth writing first, and it is **not** built on NetArchTest despite earlier drafts saying
so. The kernel already exposes `Chain.Links`, `Chain.Describe()` and `LinkDescriptor`, so the tests inspect
the *actual declared chains* by reflection rather than type shapes — strictly better, and one dependency
fewer.

| Test | What it catches |
| --- | --- |
| `Every_declared_chain_builds` | Reads each static `Chain` field, which forces the type initializer, which runs `Build()` and the wiring check. Every ordering error in every chain, in milliseconds. |
| `Nothing_follows_the_saving_link` | Rule 10. Skips chains with no `SaveChanges` — read-only and side-effect-only chains are legal. |
| `No_link_depends_on_another_link` | Rule 5, via constructor parameters. |
| `Every_link_in_every_chain_is_registered` | Resolution failures, including the open-generic `ValidateRequest<>` case. |
| `Kernel_does_not_reference_the_api` | `FHS.Chain` stays application-free. |
| `Every_entity_except_the_outbox_has_a_concurrency_token` | A new entity added without an `xmin` mapping. |
| `No_migration_creates_the_xmin_system_column` | A regenerated migration reintroducing `AddColumn("xmin")`, which Postgres rejects. |

**`Every_declared_chain_builds` is the load-bearing one**, and it does its work as a side effect: it is the
only thing that makes the wiring check a *build-time* guard, since with the `IEndpoint` pattern the static
initializer otherwise waits for the first request (Rule 13).

Two caveats, stated so nobody assumes more coverage than exists. `No_link_depends_on_another_link` checks
constructor parameters, which catches the realistic violation — links are stateless with constructor
injection, so there is no other way to obtain one — but would miss a link `new`-ed inline; real reference
analysis needs IL inspection. And **nothing enforces the `AsNoTracking` / no-`ExecuteUpdate` half of Rule
14**, which is a call-site property rather than a type property; that stays a review rule until it bites.

Three levels from the original design are gone, and all three for the same reason — the thing they tested
no longer exists as a separate mechanism. (A fourth, the link tier, is gone too, but for a different reason;
that one is recorded below.)

- The **chain wiring test** is now `Build()`, reached by the architecture test above.
- The **save-boundary test** enforced Rule 10 against `.OnCommitted<>()`, and v1 has no post-commit segment.
- The **chain shape test** (`Describe()` equals an expected list) is optional. Write one for a chain whose
  sequence carries business meaning worth pinning; skip it otherwise.

**Integration tests are built** (2026-09-01) — one class per endpoint across all six, on Testcontainers
Postgres. Two conventions were added while writing them and are worth knowing before extending the tier.
Persisted state is asserted by projecting the entity onto a **snapshot record** and comparing it whole, so
one failure names every wrong member at once; `SnapshotCoverageTests` then reflects over every
`ISnapshotModel` and fails when an entity grows a property no snapshot accounts for. And the API's
`TimeProvider` is replaced with a `FakeTimeProvider`, which is what lets `CreatedAt`, `ResolvedAt` and
`OccurredAt` be asserted as values rather than excluded as noise.

**The link tier is cut** (2026-09-01), and with it the fourth test project it would have needed. §11
originally called link tests "where most coverage should live", but that was written before the integration
tier existed and it did not survive contact with one. Chains are thin, every link sits on a path some
endpoint already exercises, and a link that changes shape fails an integration test the same day — so the
tier would have duplicated coverage rather than added it, at the cost of an EF fake and a set of test
doubles kept in step with `ICurrentUser` and `IActorDirectory` forever.

What that gives up, stated so nobody assumes otherwise: branches that are expensive or impossible to reach
over HTTP are not covered. `SaveChanges` turning a `DbUpdateConcurrencyException` into
`Concurrency.Conflict` needs two genuinely racing requests and has no test today, and
`Errors.Unauthenticated()` in `ResolveActor` cannot be reached through an authenticated endpoint at all.
Both are accepted. If a link ever grows real branching logic — Rule 3 pressure, in §13's terms — the answer
is a test for *that link*, not rebuilding the tier.

Note for whoever runs these: xUnit v3 uses Microsoft.Testing.Platform, which the .NET 10 SDK will not drive
through the old VSTest bridge. `dotnet run --project tests/Fhs.ArchitectureTests` works today; `dotnet test`
needs the repo-root opt-in.

---

## 12. Assumptions — all four settled

**Status: closed (2026-08-29).** Every load-bearing assumption behind the global-link idea has now been
verified against a compiler and a running process. This section is kept as a record of what was actually at
risk, because "it compiles" was doing a lot of work in earlier drafts and it is worth remembering that none
of this was obvious in advance.

| # | Assumption | Settled by |
| --- | --- | --- |
| 1 | A contravariant conversion satisfies a generic constraint — `ResolveActor : ILink<IHasActor>` satisfies `where TLink : class, ILink<TState>` when `TState : IHasActor` | `CreateDefect` compiling |
| 2 | Local and global links compose in one builder | `CreateDefect` compiling |
| 3 | The runtime cast holds — `(ILink<TState>)services.GetRequiredService(...)` succeeds via variant `castClass` | The first successful `POST /defects` |
| 4 | `ChainState<TResult>` does not interfere with the capability conversions | `CreateDefect` compiling |

(1), (2) and (4) fell together the first time `CreateDefectEndpoint` built — the constraint is checked where
it is *applied*, so the chain declaration is the only thing that could have tested them. (3) needed a
request, because the CLR performs the variant cast at run time and nothing earlier exercises it.

Had (1) or (2) failed, §5 and §7 would have changed shape and the global-link idea would have been dead in
its current form. They did not. `ValidateRequest<T>` later added a fifth case for free — a link whose
capability is itself generic, `ILink<IHasRequest<TRequest>>` — which works the same way.

---

## 13. Risks, and how we will know

**This is a proof of concept. Deciding to abandon the pattern is a successful outcome of the POC**, as
long as the decision is made on evidence. Concrete criteria, to be reviewed once roughly ten endpoints
exist:

| Signal | Keep | Abandon |
| --- | --- | --- |
| A developer who has never seen a feature explains it from the declaration alone | under ~2 minutes | needs to open the links anyway |
| Share of endpoints that are chains rather than plain handlers | all of them, per Rule 1 — read the escape-hatch row instead | plain handlers reappearing because a feature fought the chain |
| Escape-hatch count (§8) | rare and explainable | routine |
| `[Requires]`/`[Produces]` ceremony | reads as useful documentation on the link | felt as noise, or quietly omitted until `Build()` complains |
| Rule 3 pressure (features wanting a branch) | rare | constant |
| Kernel size | stays near 300 LOC (275 at kernel completion) | keeps growing to accommodate features |
| Capability interfaces | three to six, stable | proliferating, one per link |
| Median chain length, **write chains only** | 3–8 | drifting above 8 |
| A real production stack trace | readable | unusable |

Two rows were amended on 2026-09-01, when Rule 1 stopped exempting reads. Every endpoint is a chain now,
so counting chains no longer measures anything — the escape-hatch row is what carries that signal. And
read chains are one link by construction, so including them would drag the median toward 2 for a reason
that says nothing about whether the pattern fits. Measure the median over write chains, and watch the
read side's file count under the ceremony-floor risk below instead.

### The ten-endpoint review (2026-09-03)

The trigger in the paragraph above fired: the read side took the count to ten. **The verdict is keep**,
and the six measurable rows are the reason — not one of them points the other way.

| Signal | Measured | |
| --- | --- | --- |
| Escape-hatch count | **0.** Every route is an `IEndpoint` with a chain; the only `Map*` calls outside a slice are `MapDefaultEndpoints`/`MapApiEndpoints` | Keep |
| Kernel size | **Unchanged since 2026-08-29** — not one commit to `backend/FHS.Chain` across six new endpoints. The freeze held without being enforced | Keep |
| Capability interfaces | **3**, the v1 seed set, none added | Keep |
| Median write-chain length | **4** (7, 6, 4, 4, 3, 3) | Keep |
| Rule 3 pressure | **0.** Not one conditional-skip link in any slice | Keep |
| `[Requires]`/`[Produces]` ceremony | **10 attributes across 6 links**, every one a real hand-off. The four read chains carry none, because a one-link chain has no boundary to declare | Keep |

The three judgment rows are recorded as they stand rather than scored: the explain-it-in-two-minutes test
has not been run against a second developer, because there is not one yet; and the production-stack-trace
row is unanswerable while there is no production. Both are live again at the next review.

**The strongest single result is the kernel row.** The worry in §13's first standing risk is that you end
up maintaining a framework — that features arrive and the kernel grows to meet them. Six endpoints across
three aggregates, two chain kinds and both a read and a write side arrived, and the kernel did not move at
all. That is the difference between a kernel that fits and one that is being bent.

**The weakest is the ceremony floor**, and it is exactly where §13 predicted. Read slices are four files
each against six or seven for writes, and reads are now four of ten slices — for a system whose users
mostly *look* at defects, that ratio moves the wrong way from here. Nothing is being done about it yet;
the deferred `feature-slice` scaffold (§14) is the answer if it starts to bite, and this is the row to
watch at the next review.

Three decisions came out of the review, and all three are recorded where they belong rather than here:
`MapChain` was decided against rather than deferred again (§7, §14); read paging was settled as a
bounded/unbounded question rather than a consistency one (§9); and the two reference-data commands got the
actor and the domain event they were missing (§15).

### The seventeen-endpoint check (2026-09-05)

Not a full review — the criteria were reviewed at ten and the verdict stands. This records what the
Escapes and Customers milestone did to the numbers, because it is the first evidence from an aggregate
family the pattern was **not** designed against.

| Signal | At ten | At seventeen |
| --- | --- | --- |
| Kernel size | unchanged since 2026-08-29 | **still unchanged** — not one commit to `backend/FHS.Chain` |
| Capability interfaces | 3 | **3** |
| Median write-chain length | 4 | **5** (4, 4, 4, 5, 5, 5, 6, 6, 7, 7) |
| Escape-hatch count | 0 | **0** |
| Rule 3 pressure | 0 | **0** |
| Endpoints | 10 | **17** — 10 write chains, 7 read |
| Integration tests | 39 | **65** |

**The kernel row is the finding.** Two new entities, seven endpoints, a new aggregate family with its own
lifecycle, and `FHS.Chain` did not move — nor did `Program.cs`, nor any capability interface.
`ValidateRequestInput`, `ResolveActor`, `RecordDomainEvents` and `SaveChanges` were reused unmodified
across a third family. §13's first standing risk is that you end up maintaining a framework that grows to
meet each feature; seven features arrived and it grew by nothing.

`CreateEscape` mirrors `CreateDefect` at seven links with `LoadAndEnsureCustomerExistence` standing where
`LoadAndEnsureStationExistence` stands. That is the clearest available demonstration that the shape
generalises rather than having been fitted to Defects.

The ceremony-floor row moved the wrong way, as predicted: seven new slices at four to seven files each,
and reads are now 7 of 17 endpoints. Still watched, still not acted on. §14's `feature-slice` scaffold is
the answer if the typing becomes the complaint.

### Standing risks

- **You are maintaining a framework.** Cap it, freeze it after v1. Growth is the smell that says the
  pattern is being pushed past its fit. Everything in §14 is a *candidate*, not a roadmap — each one needs
  a real feature demanding it.
- **Metadata drift** — the wiring check verifies ordering against what links *declare*, and a link could
  declare `[Produces]` for a field it never assigns. Much smaller than the unverified ordering it replaces;
  closing it needs the analyzer, which v1 does not build.
- **Ceremony floor** — a trivial feature does not deserve four files. Rule 1 used to mitigate this and
  since 2026-09-01 it *causes* it: a read slice is State, Models, one link and Endpoint, for what a plain
  handler did in two files. That was accepted deliberately, for the reasons in Rule 1, and it is the
  standing cost of the uniformity decision. It is worth watching if the read side grows faster than the
  write side, which for a system whose users mostly *look* at defects is the likely direction.
- **Not a workflow engine** — worth repeating, because the vocabulary invites the assumption. No
  durability, no resume, no compensation.

---

## 14. Deferred

Nothing here is rejected; each is waiting for a feature that demands it. **Do not build any of it
speculatively** — the point of v1 is to find out which of these the codebase actually asks for. The tables
below are now the record: the reasoning they summarise used to live in the deleted companion document, so
a row's "Why" and "Trigger to revisit" are all there is, and both are worth writing properly.

### Cut from the original design for v1

**One row has already come back.** `Chain<TState, TResult>`, `ChainState<TResult>`, `Produce()` and
`Result<T>` were deferred here on a cost estimate — six doubled kernel types — that inheritance made
wrong. They are in the kernel as built; see §7 for what they cost and what they bought.

| Deferred | Why | Trigger to revisit |
| --- | --- | --- |
| `.OnCommitted<>()` | **Superseded, not pending.** Its whole job was keeping a post-commit failure off the response, and a post-save link that only enqueues has no failure to keep off it (§6) | A post-commit action that must be able to fail the request, or enough post-save links that Rule 10 stops being reviewable by eye |
| `.Link<T>(when: …)` | Predicate in the descriptor, branch in the runner, `IsConditional` special case in the wiring check | A feature with a genuinely optional link — and a conditional link is never a guaranteed producer, so the check needs teaching |
| `IChainWrapper`, `ChainRun` | Exactly two wrappers exist, neither varies per feature, both are five lines inline (§6) | A third around-concern, or one that must vary per chain |
| `ITransactional` + explicit transaction wrapper | Multi-table atomic writes are the exception; no feature needs one | A feature needing several saves to succeed or roll back together. Note: with `EnableRetryOnFailure` on Npgsql, manual `BeginTransaction` throws unless it goes through `ExecutionStrategy.ExecuteInTransaction` |
| `IChainCatalog`, `GET /_chains` | Its two jobs were the wiring check (now in `Build()`) and a dev endpoint | Wanting the dev endpoint. `Describe()` already exists, so this is small |
| ~~`MapChain`~~ | **Decided against, 2026-09-03** — its trigger fired at ten endpoints and the answer was no. The `runner.RunAsync(Handle, …)` line it would absorb is what keeps the endpoint file the place the feature is legible (§7) | Closed. Reopen only if the endpoint delegates start carrying real logic worth deduplicating |
| `IHasIdempotencyKey` | Rule 9 — zero users | The second feature that needs idempotent replay |
| Deriving global links' `Requires`/`Produces` from their capability interface | `[Produces(nameof(IHasActor.Actor))]` gets the same result with no reflection, on about three shared links (§5) | Enough shared links that the attribute becomes real duplication |

### Already deferred in the original design

- **A Roslyn analyzer**, **explicitly not v1.**
  Ordering is already handled by the declarations in §5 and the `Build()` check, and *that check needs no
  Roslyn at all*. The analyzer's one unique remaining job is proving declarations truthful — catching a
  link that declares `[Produces(Station)]` and never assigns it (`FHS001`). Everything else it would
  enforce has a runtime equivalent already. Revisit only if metadata drift turns out to bite in practice.
  Rule 13 keeps it buildable in the meantime, at zero cost today.
- **`Produced<T>`** — a struct wrapping produced State fields so reading one before any link wrote it
  throws a named diagnostic instead of a `NullReferenceException`. Still deferred, and now with less to
  do: the wiring check prevents the ordering failure outright, and `ChainState<TResult>.Result` already
  throws `ChainResultException` for the one field that matters most (§7). What is left uncovered is
  intermediate hand-off fields, where `CS8602` is the guard.
- **A `feature-slice` scaffold** that generates the folder, State, chain declaration and endpoint, so the
  ceremony floor is paid by a generator rather than by hand.
- **Frontend architecture as a written convention.** The technology is settled and the SPA is built (§1),
  but it is organised by component kind — `api/`, `utils/`, `routes/`, `components/{controls,pages,…}` — not
  by vertical slice, so the two sides do not currently share one mental model. Whether they should is still
  open, and it is a separate document either way.
- **Parallel links.** Deliberately excluded: it breaks the linear-reading property that is the entire
  point. Revisit only with a measured performance reason.

---

## 15. Build order

Written by hand, in order, per the working agreement in §1. The project is **not run between steps** — it
is run once, at the end.

**The build order is complete as of 2026-08-29**, including the single end-to-end run it was structured
around. `POST /defects` returned `201` against Postgres and Keycloak under Aspire.

| # | Step | Status |
| --- | --- | --- |
| 1 | The §12 spike | **skipped** — folded into step 4, since the assumptions are only checkable at a real call site |
| 2 | `FHS.Chain` — §7's table, no application references | **done** (2026-08-23) |
| 3 | **Foundation** — `AddChain`, entities, the two DbContexts + migrations, Keycloak auth, the outbox table, capabilities and the shared links, `Program.cs` wiring | **done** |
| 4 | **One feature end to end** — `CreateDefect`, per §9. Compiling this settled §12 (1), (2) and (4) | **done** |
| 5 | **The architecture tests** (§11) — seven, green | **done** |
| 6 | **The second feature** — `ResolveDefect`, which proved the shared links really are shared | **done** |
| 7 | **The first run** — settled §12 (3), the runtime variant cast | **done** (2026-08-29) |

Step 3 grew considerably beyond its original description, and the additions are worth naming because none
were anticipated: the command/query context split (§6), FluentValidation as a first link rather than a
filter (Rule 7), the `AsNoTracking`-plus-explicit-write convention (Rule 14), and optimistic concurrency via
`xmin` on every mutable entity.

**What is not done**, in the order it is worth doing:

1. ~~**Integration tests**~~ — **done** (2026-09-01), all six endpoints. Writing them found two API bugs
   the other tiers could not see: binding failures returned `500` in development and a bodiless `400` in
   production, because `RouteHandlerOptions.ThrowOnBadRequest` defaults to `IsDevelopment()` and nothing
   caught the resulting `BadHttpRequestException`. Both are fixed — the flag is pinned in every
   environment and a `BadHttpRequestExceptionHandler` renders the ProblemDetails.
2. ~~**The read side**~~ — **done** (2026-09-03): `GET /defects`, `GET /defects/{id}`, `GET /stations`,
   `GET /error-codes`. It is the first use of `FhsQueryDbContext`, which had been registered and
   referenced by nothing since step 3, and writing it is what reversed Rule 1 (§8). Paging is offset-based
   and clamped rather than validated; keyset paging on the v7 `Id` is the escape if the defect table ever
   outgrows it. Only `GET /defects` pages at all — see §9 for why the other two do not.
3. ~~**The §13 review**~~ — **done** (2026-09-03). Verdict: keep. The measured rows and what came out of
   it are in §13.

**Carried out of the review** — both **done** (2026-09-04):

1. **Actor and domain events on the two reference-data commands.** `RetireErrorCode` and
   `DecommissionStation` mutate state with no `ResolveActor` and no `RecordDomainEvents`, so nothing
   records who retired a code or decommissioned a station. Both chains gain `ResolveActor` first and
   `RecordDomainEvents` before the save, and both states gain `IHasActor` and `IRaisesEvents` — the same
   shape `ResolveDefect` already has, taking each chain from three links to five.

   **The actor is carried on the event, not on the entity.** `Station` and `ErrorCode` get no
   `RetiredBy`/`DecommissionedBy` columns, so there is no migration: the outbox row is the audit record.
   That is the cheaper half of the decision and it is reversible — if "who decommissioned this station"
   turns out to be a query rather than an audit trail, the columns are added then, against a table that
   already has the events to backfill from.

2. **Assert the outbox payload, not just its type.** `OutboxMessageSnapshot` excluded `Payload`, so every
   outbox assertion in the suite proved only that an event of some type was recorded at some time — never
   who did it or what it carried. That was tolerable while the actor on an event was incidental; item 1
   makes it the entire point, so the gap closes with it.

   The snapshot gains a `Payload` member holding the deserialized `IDomainEvent`, and `From` resolves the
   concrete type by name off `OutboxMessage.Type` before deserializing with the same options the API
   serialized under — web defaults plus `JsonStringEnumConverter`, reused from the test suite's existing
   `JsonExtensions` rather than redeclared, because the two agreeing is the property under test. Equality
   works through the interface because every event is a `record`.

   **The durable half is that `Payload` leaves `ExcludedProperties`.** `SnapshotCoverageTests` reflects
   over every entity property and fails on any the snapshot does not account for, so once the exclusion is
   gone the payload cannot silently stop being asserted again. The assertion is the fix; the coverage test
   is what keeps it fixed.

   `For<TEvent>` also stops taking a separate timestamp and reads `OccurredAt` off the event it is given,
   which removes the one way a call site could previously assert a time that disagreed with the payload
   beside it.

### Milestone: Escapes and Customers — **done** (2026-09-05)

The domain's missing half. Seven endpoints, two entities, one migration, taking the count to seventeen and
the integration suite to 65. Verified green: `dotnet build` clean, 7/7 architecture tests, 65/65
integration tests.

| Area | Endpoints |
| --- | --- |
| Customers | `POST /customers`, `GET /customers`, `POST /customers/{id}/deactivate` |
| Escapes | `POST /escapes`, `POST /escapes/{id}/resolve`, `GET /escapes/{id}`, `GET /escapes` |

Decisions worth keeping, because each was taken against a plausible alternative:

- **An Escape links to neither a Defect nor a Station.** Both were proposed and both were rejected on the
  same ground — the inward trace-back would be unreliable often enough to mislead, and a nullable field
  that is usually null is worse than no field. See §1.
- **`GET /customers` is unpaged; `GET /escapes` is paged.** The bounded/unbounded rule from §9, applied
  rather than reasoned about again. Customers are what the SPA's dropdowns bind to, so wrapping them in a
  paged envelope would have forced a second unpaged endpoint into existence beside it.
- **`DeactivateCustomer` shipped with `ResolveActor` and `RecordDomainEvents` from the first commit** —
  the first reference-data command that did not have to be retrofitted with them. The actor rides on a
  `CustomerDeactivated` event, not on new entity columns, matching the decision above it.
- **`MapChain` stayed dead.** Seven new endpoints and the plain minimal API delegate was never the thing
  that hurt.

One as-built wart: `Features/Escapes/CreateEscape/` names its endpoint file `CreateEscapeEndpoint .cs`
— with a space, and not `Endpoint.cs` like the other sixteen slices. C# does not care and the suite is
green, but it is the one file in seventeen that breaks the convention.

Link tests were item 2 here and are **cut**, not deferred — see §11 for why and for what it costs.

Three known scars to be aware of when extending this, all of which cost time once already:

- The Keycloak realm import **only runs when the realm does not already exist**. With a persistent data
  mount, editing `fhs-realm.json` and restarting silently keeps the old realm; `temp/keycloak` has to be
  deleted first.
- `xmin` is a Postgres **system column**. A regenerated migration will try to create it and fail; the
  operation must be removed by hand. There is a test for this now.
- An index `HasFilter` string is opaque to EF, so a mismatch with the naming convention survives
  `migrations add` and only fails on apply.

Step 3 did not exist in the original build order, which went straight from the kernel to `CreateDefect`.
It was split out because the feature needs a database, an `Actor` and an events table before it can be
written at all, and discovering that halfway through step 4 would mean writing the feature twice.

**Auth is Keycloak**, orchestrated by Aspire alongside PostgreSQL, with the API validating JWTs against
it. `ICurrentUser` reads the validated principal; `IActorDirectory` maps that identity onto an `Actor`
row. Only `ResolveActor` touches either, so the rest of the design is unaffected by the choice.

**Domain events go to an outbox table, not a message bus.** Nothing subscribes yet, and §1 commits to no
broker until something needs one. `RecordDomainEvents` writes outbox rows inside the same transaction as
the data, which is the property the link exists for; the reader that drains them is a background service
added with the first real subscriber, and swapping it for a bus later changes that service and nothing
else. The rows live in their own `outbox` schema, on one table with a partial index over the unprocessed
predicate — physical separation, if the write rate ever justifies it, is `PARTITION BY LIST`, which keeps
one insert path and preserves ordering across event types.

**Cache invalidation is not in this order**, because nothing is cached yet. It arrives with the first
cached read — a post-save link plus a hosted service (§6), not a kernel feature.

Do not add anything from §14 along the way. If a step feels like it needs one, that is the signal §13 is
asking about — record it.
