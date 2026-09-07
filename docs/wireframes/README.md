# FHS Wireframes — conventions and app shell

> **Status: proposal, not a confirmed design record.** Nothing here has been agreed yet. This file exists
> so the remaining wireframes are written against one set of conventions instead of five. Once a screen is
> actually built and confirmed, the decision belongs in `chain-composition.md` or its successor — not here.

These wireframes describe **what is on each screen and where the data comes from**. They deliberately do
not choose a frontend framework, router, data-fetching library, or component kit; §1 of
`chain-composition.md` still lists that decision as open.

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
   without new backend work — see §7.

---

## 3. App shell

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│  FHS    Defects   Escapes   Reference ▾                              A. Fahmy ▾        │
├────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                        │
│   <page content>                                                                       │
│                                                                                        │
└────────────────────────────────────────────────────────────────────────────────────────┘

Reference ▾              A. Fahmy ▾
  Stations                 Signed in as andrew@…
  Error codes              ─────────────────────
  Customers                Sign out
```

**Why a top bar and not a sidebar.** Only two of the five concepts are transactional — Defects and Escapes
are where a shift's work happens; Stations, Error codes and Customers are reference data touched
occasionally by an admin. A sidebar would spend permanent horizontal space advertising three screens
nobody opens daily, and these list screens want every pixel of width they can get. The three reference
screens live behind one dropdown.

**Landing route: `/defects`,** not a dashboard. It is the screen the most frequent user needs first, and
it is the only honest choice while no aggregate endpoint exists.

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

## 6. States every screen must draw

A wireframe is not finished until all five exist. Most of the bugs in an internal tool live here.

1. **Loading** — first load. Skeleton rows for tables, not a centred spinner; the table's shape is known.
2. **Empty, no filters** — genuinely no records. "No defects have been raised yet." plus the primary action.
3. **Empty, filtered** — the filters excluded everything. Different copy, and a **Clear filters** button.
   Never show the two empty states with the same words.
4. **Error** — the request failed. Message plus **Retry**. Never an empty table pretending to be empty.
5. **Submitting** — the action button is busy and disabled; the form stays readable, not greyed out.

---

## 7. Deliberately not wireframed

- **Dashboard / analytics.** No endpoint aggregates anything. Wireframing counts and trend charts now
  would design a screen that cannot be built, and would quietly commit the backend to work nobody has
  agreed. If it is wanted, it starts as an endpoint discussion.
- **Escape → Defect traceability.** An Escape deliberately holds no link back to a Station or a Defect
  (§1, and it is a domain decision, not a gap). No screen may imply that trace exists.
- **Search.** There is no free-text search parameter on any endpoint. Filters are exact-match only.
- **Bulk actions.** Nothing accepts more than one id.

---

## 8. Remaining files

| File | Covers | Status |
| --- | --- | --- |
| `defects.md` | List, detail, create, resolve | **Written — use it as the pattern** |
| `escapes.md` | Same four screens, customer instead of station | To write |
| `reference-data.md` | Stations, error codes, customers: list, create, deactivate/retire/decommission | To write |

`escapes.md` is close to a mirror of `defects.md`: swap Station for Customer, `raisedBy/CreatedAt` for
`reportedBy/ReportedAt`, and drop any hint of a station or an originating defect. It is not identical
enough to share one document, and not different enough to redesign from scratch.
