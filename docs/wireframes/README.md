# FHS Wireframes — conventions and app shell

> **Status: as-built, confirmed 2026-09-12.** Every screen described here is implemented and running. The
> file began as a proposal, written so the wireframes shared one set of conventions instead of five; it is
> now the record of what those conventions turned out to be. Where a built screen and a drawing disagree,
> the built screen wins and the drawing is the bug.

These wireframes describe **what is on each screen and where the data comes from**. The frontend stack is no
longer an open question — see §1 of `chain-composition.md`. What ships is React 19 and TypeScript on Vite
with Bun as the package manager, react-router v8 in data mode, axios behind hand-written verb hooks,
`react-oidc-context` over Keycloak, and Tailwind v4 with `light-dark()` tokens. There is no component kit
and no codegen: the controls under `frontend/src/components/controls` are the kit, and the API models are
written by hand.

---

## 1. Notation

```text
[ Label ]        button                    (•) / ( )    radio, selected / not
[            ]   text input                [x] / [ ]    checkbox, checked / not
[         ▾]     select / combobox         ‹ ›          pagination
■ / □            severity swatch           ···          truncated text
● OPEN           status pill               ‹ Back       navigation up
*                required field
```

Frames are drawn at a fixed width for legibility only. They are not a grid spec and imply no pixel widths.

---

## 2. What the API actually supports

Every screen below has to be built out of these seventeen endpoints. There are **no others**, and the
wireframes must not assume any.

| Concept | Endpoints |
| --- | --- |
| Defects | `GET /defects` · `GET /defects/{id}` · `POST /defects` · `POST /defects/{id}/resolve` |
| Escapes | `GET /escapes` · `GET /escapes/{id}` · `POST /escapes` · `POST /escapes/{id}/resolve` |
| Stations | `GET /stations` · `POST /stations` · `POST /stations/{id}/decommission` |
| Error codes | `GET /error-codes` · `POST /error-codes` · `POST /error-codes/{id}/retire` |
| Customers | `GET /customers` · `POST /customers` · `POST /customers/{id}/deactivate` |

Four consequences shape every screen, and they are the reason this section comes before any drawing:

1. **There is no edit endpoint anywhere.** No `PUT`, no `PATCH`. A record is created, then optionally
   resolved or retired. So there are **no edit screens** — detail pages are read-only surfaces with one
   action, and there is no pencil icon anywhere in this design.
2. **Severity is never entered by the user.** `CreateDefectRequest` and `CreateEscapeRequest` carry only
   `(StationCode | CustomerCode, ErrorCode, Description)`. Severity is derived from the chosen error code
   by the `ClassifyDefect` / `ClassifyEscape` link. On a form it is **displayed as a consequence** of
   picking the error code, never offered as an input.
3. **Writes reference codes, not ids.** Pickers display a friendly label but submit `"ST-04"`, not a GUID.
4. **There is no dashboard, statistics, or trend endpoint.** A landing dashboard cannot be built today
   without new backend work — see §8.

**One gap is known rather than deliberate.** The lookup screens will want edit and delete eventually, and
neither endpoint exists; the actions column holds a single lifecycle action because that is all the API
offers. No screen shows an edit affordance until it does.

---

## 3. App shell

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│  FHS    Defects   Escapes   Lookups ▾                                A. Fahmy ▾        │
├────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                        │
│   <page content>                                                                       │
│                                                                                        │
└────────────────────────────────────────────────────────────────────────────────────────┘

Lookups ▾                A. Fahmy ▾
  Stations                 Signed in as andrew@…
  Error codes              ─────────────────────
  Customers                Sign out
```

**Why a top bar and not a sidebar.** Only two of the five concepts are transactional — Defects and Escapes
are where a shift's work happens; Stations, Error codes and Customers are looked up occasionally, mostly by
an admin. A sidebar would spend permanent horizontal space advertising three screens nobody opens daily,
and these list screens want every pixel of width they can get. The three lookup screens live behind one
dropdown, labelled **Lookups**.

**Landing route: `/defects`,** not a dashboard. It is the screen the most frequent user needs first, and it
is the only honest choice while no aggregate endpoint exists. In the built app the brand mark links there,
and no navigation entry points at `/`. The root path still exists as an index route rendering nothing; it
is held for the analytics feature, which is where a real dashboard becomes possible because it is the work
that would add the aggregate endpoints (§8).

**Admin-only actions.** The lifecycle actions on the lookup screens — decommission, retire, deactivate —
render only for a user holding the `admin` realm role, read from the `roles` claim in the ID token. A
non-admin sees the same lists without an actions column rather than a disabled button: there is nothing
they can do there, and a greyed-out control that never enables is an invitation to ask why.

---

## 4. Conventions every screen follows

**Severity.** Three values only — `Minor`, `Major`, `Critical`. One visual treatment, used everywhere:

```text
■ CRITICAL     filled swatch, strongest colour
■ MAJOR        filled swatch, mid colour
□ MINOR        outline swatch, neutral
```

Colour carries meaning here, so it never appears decoratively anywhere else in the UI. Severity is always
accompanied by its word — never colour alone.

**Status.** `● OPEN` / `○ RESOLVED`, derived from `resolvedAt` being null. There is no third state.

**Paging.** `GET /defects` and `GET /escapes` are paged: `page`, `pageSize` (default **20**, max **100**),
and the response carries `items`, `page`, `pageSize`, `totalCount`, `totalPages`. `GET /stations`,
`/error-codes` and `/customers` are **unpaged** — they return a plain list and take a single
`includeInactive` flag. Reference screens therefore filter client-side and have no pager.

**Timestamps.** `DateTimeOffset` from the API. Show time only for today (`09:41`), date and time otherwise
(`2026-09-05 14:20`). Full value on hover.

**Actors.** `raisedBy` / `reportedBy` / `resolvedBy` arrive as display names, already resolved server-side.
The UI never renders a subject id or an actor GUID.

**Theme.** Three states — system, light, dark — kept in `localStorage` and stamped onto the root element as
`data-theme`; "system" stamps nothing and lets `light-dark()` follow the OS. Every colour is a token in
`frontend/src/index.css`. No component carries a colour literal, with one deliberate exception: the modal
scrim, which is the same wash in both themes because it is not part of either.

**The dark ramp was rebuilt on 2026-09-12.** The first dark palette put every structural surface between
L\* 6 and L\* 17. A card sat at **1.12:1** against the page behind it and its hairline at **1.63:1**, so on
an uncalibrated panel or in a bright room the interface collapsed into one flat sheet and only the
near-white buttons survived. Text contrast was never the problem — ink on surface measures 15:1. The
replacement lifts the whole ramp off the floor and nearly doubles the weight of the hairline that does the
real work of separating one thing from another:

| Token | Light | Dark | Why this value |
| --- | --- | --- | --- |
| `canvas` | `#e7e9e5` | `#1e221e` | L\* 12.9 — above the range a poor panel crushes to black |
| `surface` | `#fbfcf9` | `#2a2f2a` | L\* 18.8 |
| `surface-subtle` | `#f1f3ef` | `#343a34` | filter strips, skeleton rows |
| `surface-head` | `#edefea` | `#3d443d` | table headers |
| `surface-input` | `#fbfcf9` | `#191d19` | *below* canvas, so fields read as wells, not tiles |
| `border-subtle` | `#d8dbd6` | `#545c54` | 1.97:1 on surface, up from 1.63:1 |
| `border-strong` | `#c8ccc6` | `#656e65` | panel edges |

Two directions were considered and rejected. **True black** — a pure `#000` ground with panels lifted off
it — works where user content supplies the contrast a palette doesn't; a fault table supplies none, and it
left the panel-to-page step at 1.14:1, no better than before. **An accent hue on interactive elements**
was rejected *as a fix*: colour on small controls changes where the eye lands without making the structure
visible, and it would put a fourth hue beside critical, major and minor. It stays available as its own
decision, but it is not a palette repair.

---

## 5. Error handling contract

Every failure returns `FhsProblemDetails`: an RFC 7807 body plus a stable **`code`** string, and — on
validation failures only — an `errors[]` array of field errors.

| Status | Meaning | UI treatment |
| --- | --- | --- |
| 400 | Validation | Inline, per-field, from `errors[]`. Never a toast. |
| 403 | `Auth.Unauthenticated`, `Auth.UnknownActor` | Full-page block. A signed-in user who is not a registered actor cannot do anything; say so plainly. |
| 404 | `*.NotFound` | Full-page empty state on a detail route. |
| 409 | `*.AlreadyResolved`, `*.StationInactive`, `Concurrency.Conflict` | Inline banner on the surface that caused it, with a **Reload** action. |
| 503 | `Unavailable` | Retryable banner. |

**Branch on `code`, never on the status text.** `Defects.StationInactive` and `Defects.ErrorCodeInactive`
are both 409 and need different messages next to different fields.

**`Concurrency.Conflict` is a real state, not an edge case.** Every mutable entity carries Postgres `xmin`
as a concurrency token, so two people resolving the same defect is an ordinary Tuesday on a shop floor.
Its message already tells the user to reload; the UI must give them a button that does.

---

## 6. Mobile

One breakpoint, Tailwind's `md` (768px). Below it the wireframe's rule applies: *tables become complete,
tappable records, and actions stay close to the work.*

**One layout, not two.** The shell is responsive rather than switched. A second layout chosen by a JS media
query would remount the whole tree whenever the breakpoint is crossed — every `useGet` refires, an open
dialog or a half-typed form dies — and it would duplicate the theme toggle, the user menu and the nav tree
in two files that then drift. The one place the code does branch in JS is `DataGrid`, where a card is not a
restyled row and rendering both would double the DOM for every row; it is presentational, the data lives in
the page above it, so remounting it costs nothing.

**Lists become cards.** Columns carry a `card` role — `badge`, `code`, `primary`, `meta`, `actions` — and
the grid composes them:

```text
┌──────────────────────────────────────┐
│  ■ CRITICAL                          │   badge
│  ST-04 / EC-1021                     │   code, joined with a slash
│  Weld seam porosity on outer bracket │   primary, clamped to two lines
│  M. Haddad   09:41                   │   meta
├──────────────────────────────────────┤
│  Decommission                        │   actions, outside the tap target
└──────────────────────────────────────┘
```

A grid that declares no roles falls back to a label/value stack, so a new list is readable on a phone
before anyone has thought about its card. Row actions sit outside the card's link: nesting a button inside
an anchor is a bug waiting for the first screen that needs both. A `meta` value that would read as
ambiguous takes a label — the second timestamp says `Resolved 14:20` while the first stays bare.

**Filters collapse** behind one bar showing the current status and, when other filters are set, how many:
`Open · 1 filter`. The count matters — a collapsed bar that hides an applied station filter leaves someone
staring at a short list with no explanation. Above `md` the bar disappears and the filter row is always
open, exactly as it is today.

**Navigation moves to the bottom** — Defects, Escapes, Lookups — replacing the hamburger below `md`. The
tab bar is derived from the same nav tree as the top bar, so the two cannot disagree. The theme toggle and
the user menu stay in the header.

**Form actions pin to the bottom** on the create screens, and the tab bar stands down there rather than
stacking two bars: routes that bring their own action bar are listed in `routes/navigation.ts`. Both bars
pad for `env(safe-area-inset-bottom)`, and `main` reserves the clearance so the last field is never left
under a bar.

---

## 7. States every screen must draw

A wireframe is not finished until all five exist. Most of the bugs in an internal tool live here.

1. **Loading** — first load. Skeleton rows for tables, not a centred spinner; the table's shape is known.
2. **Empty, no filters** — genuinely no records. "No defects have been raised yet." plus the primary action.
3. **Empty, filtered** — the filters excluded everything. Different copy, and a **Clear filters** button.
   Never show the two empty states with the same words.
4. **Error** — the request failed. Message plus **Retry**. Never an empty table pretending to be empty.
5. **Submitting** — the action button is busy and disabled; the form stays readable, not greyed out.

---

## 8. Deliberately not wireframed

- **Dashboard / analytics.** No endpoint aggregates anything. Wireframing counts and trend charts now
  would design a screen that cannot be built, and would quietly commit the backend to work nobody has
  agreed. If it is wanted, it starts as an endpoint discussion.
- **Escape → Defect traceability.** An Escape deliberately holds no link back to a Station or a Defect
  (§1, and it is a domain decision, not a gap). No screen may imply that trace exists.
- **Search.** There is no free-text search parameter on any endpoint. Filters are exact-match only.
- **Bulk actions.** Nothing accepts more than one id.

---

## 9. Screen documents

| File | Covers | Status |
| --- | --- | --- |
| `defects.md` | List, detail, create, resolve | **Written — use it as the pattern** |
| `escapes.md` | Same four screens, customer instead of station | Never written; screens shipped 2026-09-12 |
| `lookups.md` | Stations, error codes, customers: list, create, deactivate/retire/decommission | Never written; screens shipped 2026-09-12 |

Both were planned as pre-build specs and overtaken by the build. `escapes.md` was always close to a mirror
of `defects.md` — swap Station for Customer, `raisedBy/createdAt` for `reportedBy/reportedAt`, and drop any
hint of a station or an originating defect — and that mirroring is what the built screens do. `lookups.md`
would describe one pattern used three times: a list with an Active/All toggle and a count, a create dialog
of plain fields, and one lifecycle action per row behind the admin role.

Neither is worth writing as a design document now. If either is written later it should be written *from*
the built screen, as `defects.md` documents a screen that exists — not as a proposal for one that already
shipped.

---

## 10. Drawings

| File | Screen |
| --- | --- |
| `01-defects-list.svg` · `08-dark-defects-list.svg` | Defects list |
| `02-defect-detail-resolve.svg` · `09-dark-defect-detail-resolve.svg` | Defect detail with the resolve dialog |
| `03-new-defect.svg` | Raise a defect |
| `04-escapes-list.svg` | Escapes list |
| `05-new-escape.svg` · `10-dark-new-escape.svg` | Report an escape |
| `06-lookups.svg` · `11-dark-lookups.svg` | Lookups, drawn as Error codes |
| `07-mobile.svg` · `12-dark-mobile.svg` | Phone list and phone form |

`index.html` is a gallery of all twelve. The dark drawings carry the 2026-09-12 palette (§4), and the
navigation in every drawing reads **Lookups** — `06` and `11` were named `reference-data` until the feature
settled on that name, and the phone tab bar was drawn as `MORE` before it was built against the same nav
tree as the top bar.

Two details in the phone drawings are older than the build and were left as drawn, because neither changes
what the screen is: the filter disclosure is a `›` where the build rotates a chevron, and the drawn filter
bar shows only the unfiltered summary, `Open`, where the build appends a count once other filters are set.
