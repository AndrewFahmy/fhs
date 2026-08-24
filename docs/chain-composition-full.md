# Chain Composition — FHS Architecture (complete design, archived)

> Status: **reference only. Do not build from this document.**
>
> This is the complete design, kept because roughly half of it is deferred rather than rejected — it is
> where the deferred parts are specified in full, and where the reasoning behind them lives.
> **[`chain-composition.md`](./chain-composition.md) is the document to build from**; its §14 lists every
> deferral, why, and what would trigger restoring it.
>
> Written before any of it was compiled. §12 lists what must be proven on day one and §13 lists the
> criteria for keeping or abandoning the approach.

---

## 1. Project context

**FHS (Fault Handling System)** logs and tracks quality faults from a production plant:

| Concept | Meaning |
|---|---|
| **Defect** | A fault caught internally, before the product leaves the plant |
| **Escape** | A fault that reached the customer — an internal control failed |
| **Station** | A position on the production line where a defect is detected or caused |
| **Error Code** | The catalogued fault taxonomy that defects are classified against |
| **Customer** | The recipient an escape is reported by or against |

### Shape

A **single .NET 10 API plus a SPA frontend, orchestrated by Aspire**. This is a deliberate simplification
of an earlier microservices design: one deployable API, one database, no message broker until something
actually needs one.

```text
FHS.AppHost          Aspire orchestration (pin the current Aspire version when scaffolding)
FHS.ServiceDefaults  telemetry, health checks, resilience defaults
FHS.Api              vertical slices + shared links + capabilities
FHS.Chain            the composition kernel (no application dependencies)
web/                 SPA (technology not yet chosen)
```

### Decided

- .NET 10 (SDK 10.0.400), Aspire for orchestration, PostgreSQL for persistence.
- Vertical Slice Architecture, with **Chain Composition** (this document) as the convention inside a slice.
- Minimal APIs. No MediatR — see §3.

### Still open

- **Frontend technology and framework — not chosen.** A prior note recommending React + Vite + TypeScript +
  Tailwind + shadcn/ui is **stale and non-binding**; the decision is being revisited. This document is
  backend-only and does not depend on the outcome.
- Local Kubernetes hosting approach. Currently Aspire on Docker Desktop; decide when it is actually reached.

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
static readonly Chain<State, DefectId> Handle = Chain.For<State, DefectId>()
    .Link<ResolveActor>()          // shared
    .Link<LoadStation>()           // this feature
    .Link<EnsureStationActive>()   // this feature
    .Link<ClassifyDefect>()        // this feature
    .Link<RaiseDefect>()           // this feature
    .Link<RecordDomainEvents>()    // shared
    .Link<SaveChanges>()           // shared — commit boundary
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

**This is closed, not merely mitigated.** Local links declare which State fields they consume and produce
(§5), and a wiring check verifies every chain against those declarations at startup and in a test (§11).
Reorder `LoadStation` after `ClassifyDefect` and you get:

```text
ChainWiringException: CreateDefect: 'ClassifyDefect' requires 'Station',
which no earlier link produces.
```

The check runs over every chain at once, so no feature has to remember. Two supporting notes:

- **Nullable reference types are still required.** With `Station? Station { get; set; }`, every read site
  raises `CS8602`. **`<Nullable>enable</Nullable>` with `CS8602` escalated to an error is required in this
  project.** The wiring check verifies order; NRT verifies that each individual read was thought about.
- **The residual is metadata drift** — a link could declare `[Produces(Station)]` and not assign it. That
  is a much smaller assumption than unverified ordering, and closing it is the analyzer's job (§15), which
  cross-checks each declaration against the method body.

So the design does not trade enforcement for visibility after all. It gets both, at the cost of one or two
attributes on the local links that participate in a hand-off.

---

## 3. What this is, and what it is not

**It is** a feature-scoped application of Pipes-and-Filters with Railway-Oriented error handling. That
combination has plenty of precedent — Elixir's `Plug`, Ruby's `interactor` gem, Spring Batch — so the
failure modes are known and documented here rather than discovered later.

**It is not:**

| Not this | Because |
|---|---|
| A CI/CD pipeline | Unrelated. This is why the words *pipeline*, *stage* and *job* are banned in §4. |
| A workflow engine | **No durability, no persistence, no resume-after-crash, no compensation.** A chain lives and dies inside one HTTP request. If a feature needs durable orchestration it needs Temporal or Elsa, not this. |
| MediatR pipeline behaviors | Those are cross-cutting only and identical for every request. The interesting half here is the *per-feature* sequence, which behaviors cannot express. |
| A new architecture | It is still Vertical Slice Architecture. Chain Composition is a convention *inside* a slice. |

**No MediatR.** Its dispatch indirection works directly against this design's goal — you would trade a
readable declaration list for a `Send()` call that jumps somewhere unnamed. Its genuine value, pipeline
behaviors, is covered by wrappers (§6). It also went commercial in 2025.

---

## 4. Vocabulary

| Term | Meaning |
|---|---|
| **Chain** | The ordered list of links a feature declares. One per feature. |
| **Link** | One unit of work in that list. |
| **State** | The typed object carried through the chain, holding the request and everything links produce. |
| **Capability** | A narrow interface a State implements so shared links can target it. |
| **Wrapper** | An around-concern applied to the whole run, not between links. |
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
implementing that capability. Two consequences:

**1. Global links are written once, against only what they need.**

```csharp
public sealed class ResolveActor(ICurrentUser user, IActorDirectory directory) : ILink<IHasActor>
{
    public async ValueTask<LinkResult> RunAsync(IHasActor state, CancellationToken ct)
    {
        var actor = await directory.FindAsync(user.Id, ct);
        if (actor is null) return LinkResult.Fail(Errors.UnknownActor(user.Id));

        state.Actor = actor;
        return LinkResult.Continue;
    }
}
```

No generics, no dictionary, no casting, and the signature declares its entire surface area: this link
touches `Actor` and nothing else.

**2. Local and global links coexist in one declaration list.**

```csharp
public sealed class LoadStation(FhsDbContext db) : ILink<CreateDefect.State>   // local: sees everything
public sealed class ResolveActor(...)            : ILink<IHasActor>            // global: sees one field
```

Both satisfy `where TLink : ILink<TState>` when the builder is over `CreateDefect.State`. The endpoint
declaration reads uniformly; the coupling difference is visible only in each link's own signature, which
is exactly where it belongs.

**3. The compiler enforces it.** Declaring a global link whose capability the State does not implement is
a *build error*, not a runtime surprise. The entire class of "missing context key" bug does not exist.

### Capability interfaces

Keep them narrow and few. Seed set:

```csharp
public interface IHasRequest<out TRequest> { TRequest Request { get; } }
public interface IHasActor                 { Actor Actor { get; set; } }
public interface IHasIdempotencyKey        { string? IdempotencyKey { get; } }
public interface IRaisesEvents             { List<IDomainEvent> Events { get; } }
public interface ITransactional;            // marker — opts into an explicit transaction (rare, see §6)
```

**A link has exactly one `TState` type parameter.** So a global link needing two capabilities must target
a composite interface that the State declares *explicitly* — C# interface implementation is nominal, so
implementing both parents is not enough:

```csharp
public interface IAuditedWrite : IHasActor, IRaisesEvents;

public sealed class State : ChainState<DefectId>, IAuditedWrite, IHasIdempotencyKey { ... }
//                                                ^^^^^^^^^^^^^ must be listed by name
```

Treat that tax as a feature, not a wart. It is the forcing function behind Rule 8 (§8): if a link wants
three capabilities, it probably wants constructor injection instead.

### Local links declare their dependencies

A global link's surface is declared by its type: `ILink<IHasActor>` says "I touch `Actor` and nothing
else," and the compiler holds it to that. A **local** link has no such declaration — `ILink<State>` says
only "I can see everything," which is why ordering between local links is the one thing the type system
cannot check (§2).

So local links state it directly:

```csharp
[Produces(nameof(CreateDefect.State.Station))]
public sealed class LoadStation(FhsDbContext db) : ILink<CreateDefect.State>

[Requires(nameof(CreateDefect.State.Station))]
[Produces(nameof(CreateDefect.State.Classification))]
public sealed class ClassifyDefect(IErrorCodeCatalog catalog) : ILink<CreateDefect.State>
```

This is not a second mechanism bolted on — it is the same information a capability interface carries,
expressed the only way available to a link that legitimately needs the whole State. Global links need no
attributes: their `Requires`/`Produces` are derived from the capability interface they target.

**`nameof`, not marker interfaces.** `IRequires<IHasStation>` would force a capability interface per
field, exploding the count that Rule 8 exists to keep small. `nameof` gives refactor-safety and
compile-checked existence without inventing types.

**Only declare what crosses a link boundary.** A link reading `state.Request` — always present, set by the
constructor — declares nothing. The attributes describe hand-offs between links, and nothing else.

Together with the wiring check in §11, this is what closes the ordering problem. The check is a set
difference over the declared order:

```csharp
var produced = new HashSet<string>();

foreach (var link in chain.Links)
{
    foreach (var required in link.Requires)
        if (!produced.Contains(required))
            throw new ChainWiringException(
                $"{chain.Name}: '{link.Name}' requires '{required}', which no earlier link produces.");

    if (!link.IsConditional)        // a when:-guarded link is not a guaranteed producer
        produced.UnionWith(link.Produces);
}
```

Three things fall out of that loop for free:

- **Mutual dependencies are caught.** If two links each require what the other produces, whichever runs
  first fails on an unproduced requirement.
- **Rule 4 is enforced** — two links declaring `[Produces]` for the same field in one chain is a violation
  detected in the same pass.
- **Conditional links cannot satisfy a requirement.** A `.Link<T>(when: …)` link may not run, so it never
  counts as a producer. This sharpens Rule 3 rather than working around it.

Note what is *not* required to make this work: no Roslyn analyzer, no source generation, no reflection over
method bodies. Just the declarations and thirty lines over the chain catalogue.

---

## 6. Execution model: flat loop plus wrappers

Two layers, deliberately separated.

### Links run in a flat loop

The runner iterates an array. It does **not** build a nested delegate chain the way ASP.NET middleware
does. That choice is about one thing: when something throws in production at 3am, the stack trace should
be readable rather than forty frames of `<RunAsync>d__7.MoveNext()`.

```csharp
foreach (var descriptor in chain.Links)
{
    using var activity = ChainTelemetry.Source.StartActivity($"link {descriptor.Name}");

    var link = (ILink<TState>)scope.GetRequiredService(descriptor.Type);
    var outcome = await link.RunAsync(state, ct);

    activity?.SetTag("link.outcome", outcome.Kind);

    if (outcome.Kind is LinkOutcome.Fail) return Result.Fail(outcome.Error!);
    if (outcome.Kind is LinkOutcome.Done) break;
}
```

### Around-concerns are wrappers, not links

A few concerns genuinely surround the whole run rather than sitting between links:

```csharp
public interface IChainWrapper
{
    int Order { get; }
    Task<Result> WrapAsync(ChainRun run, Func<Task<Result>> next, CancellationToken ct);
}
```

`ChainRun` carries the chain name and the state as the non-generic `ChainState` base, so wrappers can
type-test for markers without generics.

Seed wrappers, in order — deliberately only two:

| Order | Wrapper | Responsibility |
|---|---|---|
| 0 | `ExceptionShield` | Unhandled exception → `Error(Unexpected)`, logged with the chain and link name |
| 10 | `ChainTelemetry` | The parent span for the whole run |

Wrappers are configured once, globally. They are not declared per feature — a concern that varies per
feature is a link.

### The commit boundary

**EF Core's `DbContext` is already the unit of work**, and `SaveChangesAsync` wraps all tracked changes in
an implicit transaction. An explicit transaction buys nothing for a single-save feature, so there is no
unit-of-work wrapper and no `CommitChanges` link. **The feature's own saving link commits.**

That leaves one real hazard, and it is a correctness bug rather than a matter of taste: if the saving link
sits at position 5 of 7 and link 6 fails, **the write is already committed and the client gets an error
response for an operation that persisted.** Hence Rule 10:

> **The saving link is the last link that can fail the request.**

Which raises the question of where side effects that must happen *after* the commit go — notifications,
cache invalidation, outbound messages. They go in a distinct segment, declared with a different builder
method so the boundary is visible when you read the chain:

```csharp
    .Link<RecordDomainEvents>()          // writes outbox rows into the DbContext
    .Link<SaveChanges>()                 // ← commit boundary; last link that may fail
    .OnCommitted<NotifySupervisor>()     // ← runs only if everything above committed
```

| Segment | If it fails |
|---|---|
| `.Link<>()` before the save | Nothing persisted, error response |
| the saving `.Link<>()` | Nothing persisted (EF's implicit transaction), error response |
| `.OnCommitted<>()` | **The write is already committed.** Logged and surfaced for retry; the response does not change |

`.OnCommitted<>()` links use the same `ILink<TState>` contract — the *builder method* carries the
semantics, keeping the declaration the single source of truth. A `Fail` returned there is logged by the
runner, never propagated, because there is nothing left to undo.

`SaveChanges` itself is a **shared** link and needs nothing from State at all, because EF is already
tracking what earlier links added or modified:

```csharp
public sealed class SaveChanges(FhsDbContext db) : ILink<ChainState>
{
    public async ValueTask<LinkResult> RunAsync(ChainState state, CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
        return LinkResult.Continue;
    }
}
```

Targeting the `ChainState` base means every State satisfies it, so one class serves every feature — the
capability approach at its cleanest, with a state surface of zero.

This is also why the shared link is named `RecordDomainEvents` and not `DispatchDomainEvents`. It records
events as outbox rows *before* the save, so they commit atomically with the data; actual dispatch happens
after commit, by a background reader or an `.OnCommitted<>()` link. The name makes the required position
obvious.

**Multi-table atomic writes are the exception, not the rule.** A feature needing several saves to succeed
or roll back together opts in by implementing `ITransactional`, which activates an explicit transaction
wrapper. Note for when that day comes: with `EnableRetryOnFailure` enabled on Npgsql, a manual
`BeginTransaction` throws unless it goes through `ExecutionStrategy.ExecuteInTransaction`. That constraint
belongs to the opt-in path only and does not affect the default one.

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

`Done` matters: a cache hit or an idempotent replay must be able to stop the chain *successfully*.
`ErrorKind` is mapped to an HTTP status in exactly one place and surfaced as `ProblemDetails`. Exceptions
are for bugs and infrastructure faults only — never for expected business outcomes.

`Fields` exists from day one on purpose. FluentValidation reports every shape failure at once, which is
what a form needs, while a business-rule link reports one and stops. Both must produce the same
`ValidationProblemDetails` shape so the client has a single error contract regardless of which step
rejected the request — and widening `Error` later means touching every link that constructs one.

---

## 7. Two chain kinds, one link contract

A chain either produces a value or it does not. Both kinds exist as distinct types.

```csharp
public abstract class ChainState;                          // void chains

public abstract class ChainState<TResult> : ChainState      // value-producing chains
{
    public bool HasResult { get; private set; }
    public TResult Result { get; private set; } = default!;

    public void Produce(TResult result)
    {
        Result = result;
        HasResult = true;
    }
}

public sealed class Chain<TState>          where TState : ChainState;
public sealed class Chain<TState, TResult> where TState : ChainState<TResult>;

// ChainRunner
Task<Result>          RunAsync<TState>(Chain<TState>, TState, CancellationToken);
Task<Result<TResult>> RunAsync<TState, TResult>(Chain<TState, TResult>, TState, CancellationToken);
```

The value is produced **through State, via `Produce()` — not returned by a terminal link.** Three reasons
that choice matters:

- **`ILink<in TState>` stays the single link contract for both kinds.** `ResolveActor` and
  `RecordDomainEvents` are reusable across void and value-producing chains without modification. If
  links returned `TResult`, no link could ever be shared between the two and the shared catalogue would
  split in half on day one.
- **Early `Done` still produces a value.** A link that short-circuits on a cache hit calls `Produce(...)`
  then returns `Done`. A terminal-link design cannot do this, because the terminal link never runs.
- **`HasResult` makes "finished without producing" a loud failure**, not a silent `default`. The runner
  throws, naming the chain — it is a wiring bug, not a runtime condition. It also handles `struct`
  results (`DefectId`) correctly, which a nullable `TResult?` property would not.

Inheritance is spent on exactly one thing — result production. Everything else is a capability interface.
This keeps the single base-class slot from turning `ChainState` into a god base class, and does not
interfere with the contravariance in §5.

### Kernel surface

`FHS.Chain`, no application dependencies, **target ≈500 LOC**. If it grows past that, the pattern is
being pushed past its fit.

| Type | Role |
|---|---|
| `ILink<in TState>` | The one unit contract |
| `LinkResult` / `LinkOutcome` | `Continue` / `Done` / `Fail(Error)` |
| `ChainState`, `ChainState<TResult>` | State bases |
| `Chain<TState>`, `Chain<TState, TResult>` | Immutable `LinkDescriptor[]` + `Describe()` |
| `Chain.For<TState>()`, `Chain.For<TState, TResult>()` | Builders: `.Link<T>()`, `.Link<T>(when:)`, `.OnCommitted<T>()`, `.Build()` |
| `ChainRunner` | One overload per chain kind, **sharing one internal executor** |
| `IChainWrapper`, `ChainRun` | Around-concerns |
| `RequiresAttribute`, `ProducesAttribute` | Dependency declarations on local links (§5) |
| `ChainWiring.Validate(IChainCatalog)`, `ChainWiringException` | The ordering check (§5, §11) — ~30 LOC |
| `IChainCatalog` | Registry backing `GET /_chains` and the wiring check |
| `Result`, `Result<T>`, `Error`, `FieldError`, `ErrorKind` | ~70 LOC, hand-rolled, no dependency |

The two chain kinds share one executor; only result extraction at the end differs. Documented here so
nobody helpfully forks the loop later.

---

## 8. Rules and limits

These are the part that actually protects maintainability. The pattern degrades without them.

1. **Use a chain when it earns it** — three or more meaningful operations, or at least one shared concern.
   A `GET /defects/{id}` stays a plain endpoint calling a query. A mixed codebase is *correct*, not a
   failure of discipline.
2. **Three to eight links.** More than eight means the feature is really two features, or the links have
   been sliced too thin to carry meaning.
3. **One branching combinator, `.Link<T>(when: …)`.** No nested chains, no loops, no jumps. A branch
   *tree* means two features and two chains, selected at the endpoint. Linear composition handles
   branching badly — this is its best-known failure mode, and the rule is the mitigation.
4. **One writer per field, named after it, and declared.** Every State field a chain produces is nullable
   and written by exactly one link, named for what it produces: `LoadStation` → `State.Station`,
   `ClassifyDefect` → `State.Classification`. Local links declare hand-offs with
   `[Requires]`/`[Produces]` (§5); global links declare theirs through the capability interface they
   target. **This is what closes the ordering gap in §2** — the wiring check (§11) verifies every chain
   against those declarations, and one-writer-per-field is enforced in the same pass. Only declare fields
   that cross a link boundary; `state.Request` is always present and needs nothing.
5. **Links never call other links.** Doing so reinstates exactly the nested tracing this design exists to
   remove. Enforced by an architecture test, not by good intentions.
6. **Links are stateless.** Dependencies through the primary constructor, no mutable fields. They are
   resolved per request but must not rely on it.
7. **Do not absorb framework concerns.** Authentication and authorization → endpoint filters and policies.
   Request *shape* validation → a FluentValidation endpoint filter, before the chain runs. Rate limiting →
   middleware. Links carry **business** sequence only. Wanting to make everything a link is the main way
   this bloats. Validation is two steps, and the split is by *what the rule needs*:
   - **Shape**, via FluentValidation ("StationCode is required", "Description ≤ 500 chars"). Runs as an
     endpoint filter, reports every failure at once.
   - **Business state**, via links ("that station is decommissioned", "that code is already in use").
     Needs loaded data, so it cannot run before the chain.

   **Keep the FluentValidation validators synchronous and pure.** `MustAsync` with a database call is
   always available and always tempting, but a business rule hidden in a validator is invisible in the
   chain declaration — which is the entire point of the design. Sync-only also keeps the filter fast and
   free of scoped database access.
8. **Capabilities carry what the chain produces; ambient dependencies come from DI.** Clock, current user,
   tenant and configuration are constructor-injected — they are not State. This is what keeps the
   capability set small enough to memorize.
9. **Stay local until a second feature needs it.** Write `ILink<TState>` first. Promote to a
   capability-typed global link on the *second* use, never in anticipation of it. Premature capability
   interfaces are how a shared catalogue turns into a pile of one-user abstractions.
10. **The saving link is the last link that can fail the request.** Only `.OnCommitted<>()` may follow it.
    Anything that can fail after a commit produces the worst outcome available: a persisted write and an
    error response. Enforced generically by the save-boundary test (§11), not by memory.
11. **Fold existence checks into the link that loads.** `EnsureStationExists` followed by `LoadStation` is
    two round trips to learn one fact — the load *is* the check, returning `Fail(NotFound)` when it comes
    back empty. What remains for business-rule links is the checks with no downstream product: uniqueness,
    cross-field state rules, quota checks. Those earn their own links, and seeing `EnsureCodeNotDuplicated`
    in the declaration tells you the rule exists without opening anything.
12. **Every feature declares its own State.** It may inherit from `ChainState`/`ChainState<TResult>`, but
    **the base stays free of domain fields.** Every field a feature uses is declared, with its real type,
    in that feature's file. The moment a shared base carries `Station`, the property that makes a slice
    readable in isolation is gone.
13. **Keep the declaration statically analyzable.** A chain is always a static literal fluent chain in a
    field initializer — never built at runtime, never conditionally, never in a loop. And a link never
    passes `state` to another method: read it, write it, do not hand it off. Neither is needed by the
    wiring check, which reads the catalogue at runtime — they are what keeps the optional analyzer (§15)
    buildable later. Cheapest rules here to follow, most expensive to retrofit.

**Escape hatch, stated plainly:** if a feature fights the chain, write a plain handler and move on. That
is a permitted outcome, not a defeat. Record it — a growing count of escapes is the most useful signal
this document can produce (§13).

---

## 9. Anatomy of a slice

```text
src/FHS.Api/Features/Defects/CreateDefect/
    CreateDefect.cs          ← endpoint, State, and the chain declaration
    CreateDefectValidator.cs ← FluentValidation, shape only, synchronous
    LoadStation.cs
    EnsureStationActive.cs
    ClassifyDefect.cs
    RaiseDefect.cs
    NotifySupervisor.cs
```

Shared links live in `src/FHS.Api/Shared/Links/`, capabilities in `src/FHS.Api/Shared/Capabilities/`.

### Example A — value-producing chain

```csharp
public static class CreateDefect
{
    public sealed record Request(string StationCode, string ErrorCode, string Description);
    public sealed record Response(Guid DefectId);

    public sealed class State(Request request)
        : ChainState<DefectId>, IHasRequest<Request>, IHasActor, IRaisesEvents
    {
        public Request Request { get; } = request;
        public Actor Actor { get; set; } = null!;            // written by ResolveActor
        public List<IDomainEvent> Events { get; } = [];

        public Station? Station { get; set; }                 // written by LoadStation
        public Classification? Classification { get; set; }   // written by ClassifyDefect
        public Defect? Defect { get; set; }                   // written by RaiseDefect
    }

    static readonly Chain<State, DefectId> Handle = Chain.For<State, DefectId>()
        .Link<ResolveActor>()             // shared   ILink<IHasActor>
        .Link<LoadStation>()              // local    ILink<State>       — also the FK check
        .Link<EnsureStationActive>()      // local    ILink<State>       — business rule
        .Link<ClassifyDefect>()           // local    ILink<State>
        .Link<RaiseDefect>()              // local    ILink<State>       — calls Produce(defect.Id)
        .Link<RecordDomainEvents>()       // shared   ILink<IRaisesEvents>
        .Link<SaveChanges>()              // shared   ILink<ChainState>  — commit boundary
        .OnCommitted<NotifySupervisor>()  // local    ILink<State>       — cannot fail the request
        .Build();

    public static void Map(IEndpointRouteBuilder app) =>
        app.MapChain("/defects", Handle, static (Request r) => new State(r),
                (id, _) => TypedResults.Created($"/defects/{id.Value}", new Response(id.Value)))
           .WithName(nameof(CreateDefect))
           .WithTags("Defects")
           .AddEndpointFilter<ValidateShape<Request>>();
}
```

Read the declaration and you have the feature: who the actor is, that the station must exist and be
active, that the defect gets classified and raised, where it commits, and that a supervisor is notified
afterwards. Note what is *absent*: no separate FK-existence link (Rule 11 — `LoadStation` is the check),
no shape-validation link (that is the endpoint filter), no transaction ceremony, and no try/catch.

A local link, for contrast — narrow, one field, one reason to fail:

```csharp
[Produces(nameof(CreateDefect.State.Station))]
public sealed class LoadStation(FhsDbContext db) : ILink<CreateDefect.State>
{
    public async ValueTask<LinkResult> RunAsync(CreateDefect.State state, CancellationToken ct)
    {
        state.Station = await db.Stations
            .SingleOrDefaultAsync(s => s.Code == state.Request.StationCode, ct);

        return state.Station is null
            ? LinkResult.Fail(Errors.StationNotFound(state.Request.StationCode))
            : LinkResult.Continue;
    }
}
```

It reads `state.Request`, which is set by the constructor and always present, so that needs no
declaration. It produces `Station`, which a later link consumes — so that does.

### Example B — void chain, reusing the same shared links

```csharp
public static class ResolveDefect
{
    public sealed record Request(string Resolution);

    public sealed class State(Guid defectId, Request request)
        : ChainState, IHasRequest<Request>, IHasActor, IRaisesEvents
    {
        public Guid DefectId { get; } = defectId;
        public Request Request { get; } = request;
        public Actor Actor { get; set; } = null!;
        public List<IDomainEvent> Events { get; } = [];

        public Defect? Defect { get; set; }                   // written by LoadDefect
    }

    static readonly Chain<State> Handle = Chain.For<State>()
        .Link<ResolveActor>()             // shared — identical class, unmodified
        .Link<LoadDefect>()               // local
        .Link<MarkResolved>()             // local
        .Link<RecordDomainEvents>()       // shared — identical class, unmodified
        .Link<SaveChanges>()              // shared — identical class, unmodified
        .Build();

    public static void Map(IEndpointRouteBuilder app) =>
        app.MapChain(HttpMethod.Post, "/defects/{id:guid}/resolution", Handle,
                static (Guid id, Request r) => new State(id, r),
                _ => TypedResults.NoContent())
           .WithName(nameof(ResolveDefect))
           .WithTags("Defects");
}
```

`ResolveActor`, `RecordDomainEvents` and `SaveChanges` appear in both chains, unchanged, despite one chain
producing a value and the other not. That is the payoff of §7's single link contract, and it is the
specific thing the POC should confirm early.

Note the three levels of coupling visible in these two declarations, each one chosen by the link's own
signature rather than by where it sits in the list: `SaveChanges` needs nothing (`ILink<ChainState>`),
`ResolveActor` needs one field (`ILink<IHasActor>`), and `LoadStation` needs the whole feature
(`ILink<CreateDefect.State>`).

### Registration

```csharp
builder.Services.AddChainComposition(typeof(IApiMarker).Assembly);
// scans for ILink<> implementations and registers them scoped
// registers ChainRunner, the seed wrappers, and IChainCatalog
```

---

## 10. Observability and self-documentation

**One span per link.** `ActivitySource "FHS.Chain"`, tagged `chain.name`, `link.name`, `link.outcome`.
Because FHS runs under Aspire, the dashboard's trace waterfall becomes a **live diagram of the feature**,
showing the real order and the real cost of every link, per request. This is a genuine and somewhat
under-appreciated payoff of combining this pattern with Aspire: the documentation of what happened is
generated by the thing happening.

**`GET /_chains`, Development only.** Returns every registered chain and its declared links as JSON, built
from `IChainCatalog` (populated by `MapChain`, so no reflection scan and no static mutable state):

```json
[
  { "name": "CreateDefect",  "result": "DefectId",
    "links": ["ResolveActor", "LoadStation", "EnsureStationActive",
              "ClassifyDefect", "RaiseDefect", "RecordDomainEvents", "SaveChanges"],
    "onCommitted": ["NotifySupervisor"] },
  { "name": "ResolveDefect", "result": null,
    "links": ["ResolveActor", "LoadDefect", "MarkResolved", "RecordDomainEvents", "SaveChanges"],
    "onCommitted": [] }
]
```

Documentation that cannot drift, because it *is* the wiring.

---

## 11. Testing

| Level | What it covers |
|---|---|
| **Link test** | Construct a State, run one link, assert the field it wrote and the `LinkResult`. No HTTP, no DI, no database beyond a fake. This is where most coverage should live. |
| **Chain wiring test** | One test calling `ChainWiring.Validate(catalog)` over **every** registered chain, asserting that each link's `[Requires]` is produced by an earlier link (§5). This is what closes §2, and it is generic — no feature has to remember it, and a new chain is covered the moment it is registered. The same call runs at startup in Development so a bad order fails fast rather than waiting for CI. |
| **Save-boundary test** | Also generic over every chain: Rule 10, no link may follow the saving link. Catches "persisted write, error response" without any per-feature effort. |
| **Chain shape test** | Assert `CreateDefect.Handle.Describe()` equals the expected ordered list. Now that wiring is checked generically this is less critical, but it still pins *intent* — the wiring check proves an order is workable, this one proves it is the order you meant. Worth writing for chains where sequence carries business meaning. |
| **Integration test** | `WebApplicationFactory` + Testcontainers PostgreSQL, one per endpoint, asserting status codes and persisted state. |
| **Architecture test** | NetArchTest: `FHS.Chain` references nothing from `FHS.Api`; no link type references another link type (Rule 5); every registered chain's links are resolvable from the container. |

The last one is worth writing early — "every link in every chain can actually be constructed" is the
single highest-value guard against a wiring mistake, and it runs in milliseconds.

---

## 12. Assumptions to validate on day one

The design has not been compiled. Before building on it, prove these three in a throwaway project — it is
a short spike, and if any fails, §5 and §7 change shape:

1. **Variance conversion satisfies a generic constraint.** That `ResolveActor : ILink<IHasActor>` satisfies
   `where TLink : ILink<TState>` when `TState : IHasActor`. This is load-bearing for the entire design.
   It is expected to work — contravariant conversions are implicit reference conversions — but expected is
   not verified.
2. **Local and global links compose in one builder.** That `ILink<CreateDefect.State>` and
   `ILink<IHasActor>` can both be added to the same `ChainBuilder<CreateDefect.State>`.
3. **The base class does not disturb either.** That `where TState : ChainState<TResult>` and the capability
   conversions coexist on one concrete State.

Then confirm the runner's `(ILink<TState>)serviceProvider.GetRequiredService(descriptor.Type)` cast
succeeds for a link registered by its concrete type — the cast, not the resolution, is the part relying on
variance at runtime.

---

## 13. Risks, and how we will know

**This is a proof of concept. Deciding to abandon the pattern is a successful outcome of the POC**, as
long as the decision is made on evidence. Concrete criteria, to be reviewed once roughly ten endpoints
exist:

| Signal | Keep | Abandon |
|---|---|---|
| A developer who has never seen a feature explains it from the declaration alone | under ~2 minutes | needs to open the links anyway |
| Share of endpoints that are chains rather than plain handlers | meaningful majority of write paths | so few that the kernel is not paying for itself |
| Escape-hatch count (§8) | rare and explainable | routine |
| `[Requires]`/`[Produces]` ceremony | reads as useful documentation on the link | felt as noise, or quietly omitted until the wiring check complains |
| Rule 3 pressure (features wanting a branch tree) | rare | constant |
| Kernel size | stays near 500 LOC | keeps growing to accommodate features |
| Capability interfaces | roughly half a dozen, stable | proliferating, one per link |
| Median chain length | 3–8 | drifting above 8 |
| A real production stack trace | readable | unusable |

### Standing risks

- **You are maintaining a framework.** Cap it, name one owner, freeze it after v1. Growth is the smell
  that says the pattern is being pushed past its fit.
- **Metadata drift** — the wiring check (§5) verifies ordering against what links *declare*, and a link
  could declare `[Produces]` for a field it never assigns. Much smaller than the unverified ordering it
  replaces, and §15 closes it by cross-checking declarations against method bodies.
- **Ceremony floor** — a trivial feature does not deserve four files. Mitigated by Rule 1.
- **Onboarding cost** — you are asking every new developer to learn a house pattern. Mitigated by this
  document, `GET /_chains`, and eventually a scaffold generator.
- **Not a workflow engine** — worth repeating, because the vocabulary invites the assumption. No
  durability, no resume, no compensation.

---

## 14. Deferred

- **Kernel implementation.** Nothing in `FHS.Chain` is written yet.
- **A Roslyn analyzer.** See §15. Now optional rather than necessary — the wiring check (§5, §11) closes
  ordering without it. Its remaining unique value is proving declarations truthful.
- **`Produced<T>`** — a struct wrapping produced State fields so reading one before any link wrote it
  throws a named diagnostic instead of a `NullReferenceException`. **Probably now unnecessary**: it
  guarded the same failure the wiring check prevents outright. Revisit only if drift turns out to be a
  real problem in practice.
- **A `feature-slice` scaffold** that generates the folder, State, chain declaration and endpoint, so the
  ceremony floor is paid by a generator rather than by hand.
- **Frontend architecture**, once the technology is chosen. If the SPA mirrors the vertical slices, both
  sides share one mental model — but that is a separate document and a separate decision.
- **Parallel links.** Deliberately excluded from v1: it breaks the linear-reading property that is the
  entire point. Revisit only with a measured performance reason.

---

## 15. The analyzer (optional, post-POC)

Ordering is already handled by the declarations in §5 and the wiring check in §11, and **that check needs
no Roslyn at all**. A Roslyn analyzer is a later upgrade with two distinct jobs, neither of which is
urgent:

1. **Close the metadata-drift gap** — verify each `[Requires]`/`[Produces]` against what `RunAsync`
   actually reads and writes, so a declaration cannot lie. This is the only remaining hole in §2.
2. **Move the checks from startup to compile time**, and enforce several other rules the same way.

Note this is an analyzer, not a source generator. Nothing needs generating; invariants need checking.

### Why this design is unusually analyzable

1. **The chain declaration is a static, literal fluent chain in a field initializer** (Rule 13). Walking
   the `InvocationExpression` chain and reading each `TypeArgumentList` recovers the exact ordered link
   list. Nothing is constructed at runtime, so nothing has to be guessed.
2. **A link's reads and writes are recoverable from its `RunAsync` body** via `IOperation` — an
   `IAssignmentOperation` targeting `state.X` is a write, an `IPropertyReferenceOperation` on `state.X` is
   a read. Because links never pass `state` elsewhere (Rule 13), the analysis stays within one method.

### What it would enforce

Six of the thirteen rules stop being conventions:

| Rule | Diagnostic |
|---|---|
| 4 — declarations are truthful | `FHS001` — link reads or writes a State field it did not declare |
| §2 ordering | `FHS002` — `[Requires]` not produced by an earlier link (compile-time form of the §11 check) |
| 2 — length ceiling | `FHS003` — chain length outside 3–8 |
| 4 — one writer per field | `FHS004` — two links declare `[Produces]` for the same field |
| 5 — links never call links | `FHS005` — a link references another link type |
| 10 — save is last failing link | `FHS006` — a `.Link<>()` follows the saving link |
| 3 — no nested chains | `FHS007` — a chain declared inside a link |

`FHS001` is the one that cannot be done any other way, and it is the reason to eventually build this:
everything else here already has a runtime equivalent.

### Known hard cases

- **Conditional writes.** `if (x) state.Station = ...` produces on some paths only. Be conservative — and
  `.Link<T>(when:)` links are **never** guaranteed producers, which the §11 check already assumes.
- **State escaping.** If a link hands `state` to a helper method, the analysis cannot follow it. Rule 13
  forbids this, so the analyzer should **bail loudly** rather than guess: `FHS008: state escapes RunAsync;
  declarations cannot be verified`.
- **False positives are worse than the bug.** An analyzer the team disables leaves you worse off than none
  at all — and unlike the §11 check, which is exact set arithmetic over declarations, these diagnostics
  are inferred and therefore fallible. Ship them as warnings first, escalate to errors only once they are
  quiet on real code.

### Rejected: type-state in the builder

Produced fields could in principle be encoded in the builder's type parameters, so the compiler rejects a
bad order directly, with no attributes and no analyzer. C# has no type-level sets, no higher-kinded types
and no variadic generics, so this degrades into nested tuples, combinatorial overloads and unreadable
error messages. Viable in F# or Rust; **do not attempt it here.** The declarations in §5 are the practical
substitute: slightly more ceremony, vastly better diagnostics.

### Two constraints to honour now

Both are free today and expensive to retrofit. Violate them casually for six months and the analyzer stops
being buildable:

1. **A chain is always a static literal fluent chain in a field initializer.** Never constructed at
   runtime, never conditionally, never in a loop.
2. **A link never passes `state` to another method.** Read it, write it, but do not hand it off.
