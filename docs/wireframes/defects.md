# Wireframes — Defects

> **Status: proposal, not a confirmed design record.** Read `README.md` in this folder first; the notation,
> the severity and status treatments, the error contract and the five required states are defined there and
> are not repeated here.

Four screens, backed by exactly four endpoints. This is the reference set — `escapes.md` and
`reference-data.md` should follow its structure.

| Screen | Route | Endpoint |
| --- | --- | --- |
| Defect list | `/defects` | `GET /defects` |
| Defect detail | `/defects/{id}` | `GET /defects/{id}` |
| New defect | `/defects/new` | `POST /defects` |
| Resolve | dialog over detail | `POST /defects/{id}/resolve` |

---

## Visual scheme

The Defects screens support both light and dark modes. They use one semantic color system: switching mode
changes token values, never component meaning or hierarchy. The user can select `System`, `Light`, or
`Dark`; `System` is the initial default and follows the operating-system preference.

| Semantic role | Light | Dark | Use |
| --- | --- | --- | --- |
| Canvas | `#E7E9E5` | `#121412` | Viewport behind the application surface. |
| Surface | `#FBFCF9` | `#1B1E1A` | App shell, pages, tables, dialogs and form cards. |
| Subtle surface | `#F1F3EF` | `#242924` | Filter areas, secondary panels and inactive control backgrounds. |
| Table heading | `#EDEFEA` | `#292E29` | Table header row. |
| Input surface | `#FBFCF9` | `#171A17` | Text fields, comboboxes and page-jump input. |
| Primary text | `#171917` | `#F3F5F0` | Headings, body text and enabled values. |
| Muted text | `#6A706A` | `#B8BEB6` | Supporting copy, metadata and inactive navigation. |
| Table heading text | `#4A504A` | `#C7CCC5` | Column headings and compact all-caps labels. |
| Strong border | `#C8CCC6` | `#484D46` | Page, table and dialog boundaries. |
| Subtle border | `#D8DBD6` | `#3D443C` | Panel and row separators. |
| Control border | `#B8BDB6` | `#596058` | Secondary controls and un-focused fields. |
| Focus border | `#737A73` | `#899287` | Focused inputs and direct-page field. |

**Actions.** Primary commands use `#171917` on `#FBFCF9` in light mode and invert to `#F3F5F0` on
`#171917` in dark mode. Secondary controls retain the surface color and use the control border. This
keeps the active command clear without assigning product meaning to a decorative accent color.

**Severity.** Severity is the sole semantic color treatment. A swatch is always paired with its written
label; never communicate severity through color alone.

| Severity | Light | Dark | Treatment |
| --- | --- | --- | --- |
| Critical | swatch `#B22B22`; badge `#F8E2E0` with text `#7B1D18` | swatch `#EE6C61`; badge `#43221F` with text `#FFD7D3` | Filled swatch and, where space allows, a high-emphasis badge. |
| Major | swatch `#B87510` | swatch `#F1B64E` | Filled swatch; label uses primary text. |
| Minor | surface fill with `#697069` outline | surface fill with `#CAD0C7` outline | Outline swatch; label uses primary text. |

**Status.** Open is a filled dot using primary text color with a contrasting centre; Resolved is an
outline dot using muted text color. The label `OPEN` or `RESOLVED` is always rendered with the marker.
On resolved rows, only the severity marker becomes muted; the row text remains readable at normal contrast.

**Tailwind v4 implementation.** Define these as semantic CSS variables on `:root`, override them on
`[data-theme="dark"]`, and expose them through Tailwind theme tokens. Components must consume roles such
as `surface`, `ink`, `muted`, and `severity-critical`, not literal hex values. Set
`color-scheme: light dark` on the theme root so native controls agree with the selected appearance before
the React tree mounts.

---

## 1. Defect list

The screen the plant lives in. Everything about it is optimized for someone scanning many rows quickly,
several times a shift.

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│  Defects                                                            [ + New defect ]   │
├────────────────────────────────────────────────────────────────────────────────────────┤
│  Station [ All          ▾]   Error code [ All            ▾]   Severity [ All      ▾]   │
│  Status  (•) Open   ( ) Resolved   ( ) All                              [ Clear ]      │
├────────────────────────────────────────────────────────────────────────────────────────┤
│  SEVERITY   STATION   ERROR CODE   DESCRIPTION              RAISED BY      RAISED      │
├────────────────────────────────────────────────────────────────────────────────────────┤
│  ■ CRITICAL  ST-04     EC-1021     Weld seam porosity o···  M. Haddad      09:41       │
│  ■ MAJOR     ST-02     EC-0330     Torque out of spec, ···  L. Farag       09:12       │
│  □ MINOR     ST-11     EC-0075     Label misaligned         M. Haddad      08:58       │
│  ■ MAJOR     ST-04     EC-1021     Porosity, second pas···  N. Aziz        08:30       │
│  □ MINOR     ST-07     EC-0210     Surface scratch, cos···  L. Farag       08:02       │
│                                                                                        │
├────────────────────────────────────────────────────────────────────────────────────────┤
│  1–20 of 137                                        [ ‹ Prev ]   1 / 7   [ Next › ]    │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

**Row → detail.** The whole row is the target, not a link in one cell. There is no row-level action menu:
the only thing you can do to a defect is resolve it, and that needs the detail context to do responsibly.

**Column choices.** Severity leads because it drives triage. Description is the widest column and truncates
with the full text on hover — it is the only free-text field and the only one that tells you what actually
happened. Station and error code stay as codes, not names: the people using this screen read `ST-04` faster
than "Weld cell 4", and the names are one click away.

**When Status is `Resolved` or `All`,** the RAISED column is joined by RESOLVED, and resolved rows render
their severity swatch muted. Do not add a separate status column — the presence of a resolved timestamp
already says it, and a column that is empty on the default filter is wasted width.

### Filters → query string

Filters map 1:1 onto `GetDefectsRequest`. Nothing is filtered client-side.

| Control | Parameter | Notes |
| --- | --- | --- |
| Station | `stationCode` | Options from `GET /stations`. Active only — a decommissioned station can raise no new defects, though old ones remain in the data. |
| Error code | `errorCode` | Options from `GET /error-codes`. |
| Severity | `severity` | `Minor` · `Major` · `Critical`. |
| Status | `isResolved` | Open → `false`, Resolved → `true`, All → omit the parameter entirely. |
| Pager | `page`, `pageSize` | Default 20, max 100. |

**Default filter is Open.** An unfiltered list is a growing archive; the open ones are the work.

**Filters belong in the URL.** Whatever router is chosen, this screen's state must be shareable — "look at
ST-04's criticals" is a message someone sends a colleague. Changing a filter resets `page` to 1.

### States

```text
Loading            8 skeleton rows in the table's own shape. Filters stay live and usable.

Empty, unfiltered  No defects have been raised yet.
                   [ + New defect ]

Empty, filtered    No defects match these filters.
                   [ Clear filters ]

Error              Defects could not be loaded.   [ Retry ]
                   (message from the problem body)
```

---

## 2. Defect detail

Read-only. One action.

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│  ‹ Defects                                                                             │
│                                                                                        │
│  ST-04 · EC-1021                         ■ CRITICAL    ● OPEN         [ Resolve ]      │
├────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                        │
│  DESCRIPTION                                                                           │
│  Weld seam porosity on the left-hand bracket, third pass. Visible on the outer edge     │
│  after grinding.                                                                       │
│                                                                                        │
│  STATION        ST-04 — Weld cell 4                                                    │
│  ERROR CODE     EC-1021 — Weld porosity                                                │
│  RAISED BY      M. Haddad                                                              │
│  RAISED AT      2026-09-07 09:41                                                       │
│                                                                                        │
├────────────────────────────────────────────────────────────────────────────────────────┤
│  RESOLUTION                                                                            │
│  — not resolved —                                                                      │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

Resolved, the header action disappears and the footer carries the outcome:

```text
│  ST-04 · EC-1021                         ■ CRITICAL    ○ RESOLVED                      │
                                     ···
├────────────────────────────────────────────────────────────────────────────────────────┤
│  RESOLUTION                                                                            │
│  Re-welded and re-inspected. Wire feed speed corrected on the cell.                    │
│                                                                                        │
│  RESOLVED BY    N. Aziz                                                                │
│  RESOLVED AT    2026-09-07 11:02                                                        │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

**`DefectDetailResponse` carries both ids and labels** — `stationId`/`stationCode`/`stationName`,
`errorCodeId`/`errorCode`/`errorCodeDescription` — so the screen needs no second request. It renders
`CODE — Name`; the ids are for future navigation, and are not displayed.

**A resolved defect stays a page, not a dialog.** It is the record of what happened, and it gets read long
after it is closed.

### States

```text
Loading   Skeleton in the page's own shape — header, description block, field rows.

404       Defects.NotFound → full-page: "This defect no longer exists."  [ ‹ Back to defects ]

Error     Inline banner with [ Retry ].
```

---

## 3. New defect

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│  New defect                                                                     [ × ]  │
├────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                        │
│  Station *                                                                             │
│  [ ST-04 — Weld cell 4                                                             ▾]  │
│                                                                                        │
│  Error code *                                                                          │
│  [ EC-1021 — Weld porosity                                                         ▾]  │
│  ■ CRITICAL — severity comes from the error code                                       │
│                                                                                        │
│  Description *                                                            0 / 500      │
│  ┌──────────────────────────────────────────────────────────────────────────────────┐  │
│  │                                                                                  │  │
│  │                                                                                  │  │
│  └──────────────────────────────────────────────────────────────────────────────────┘  │
│                                                                                        │
├────────────────────────────────────────────────────────────────────────────────────────┤
│                                             [ Cancel ]        [ Raise defect ]         │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

**Three fields, and severity is not one of them.** The severity line appears only once an error code is
chosen, and it is text — not a control, not a disabled select. A disabled select invites people to try to
change it. Wording it as a sentence explains the rule instead of hiding it.

**Both pickers are searchable and submit codes.** `POST /defects` takes `stationCode` and `errorCode` as
strings. Options come from `GET /stations` and `GET /error-codes` with `includeInactive` omitted — a
decommissioned station and a retired error code are both rejected server-side, so offering them would be
building a dead end into the form.

**Description is 500 characters,** counted down live. That limit is enforced by `CreateDefectValidator`;
the field must not silently accept more.

### Validation and failure

| Case | Where it shows |
| --- | --- |
| 400 with `errors[]` | Inline under each named field, all at once. The chain validates shape as its first link, so a single request returns every field error — show them all, do not stop at the first. |
| `Defects.StationNotFound` (404) | Under the station field. |
| `Defects.StationInactive` (409) | Under the station field: the station was decommissioned since the list loaded. Refresh the options. |
| `Defects.ErrorCodeNotFound` (404) | Under the error code field. |
| `Defects.ErrorCodeInactive` (409) | Under the error code field, same reasoning. |
| 403 `Auth.UnknownActor` | Form-level block. The user is signed in but cannot raise defects; nothing they type will help. |

**On success** the response is `{ defectId }` only. Go to `/defects/{id}` — the operator usually wants to
confirm what they just filed, and the detail screen is the only place the derived severity and the
resolved server-side timestamp can be seen.

---

## 4. Resolve

A dialog over the detail page. Resolving is a small act with one input; leaving the defect visible behind
it is the point.

```text
┌──────────────────────────────────────────────────────────────┐
│  Resolve defect                                              │
├──────────────────────────────────────────────────────────────┤
│  ST-04 · EC-1021 · raised 09:41 by M. Haddad                 │
│                                                              │
│  Resolution *                                    0 / 500     │
│  ┌────────────────────────────────────────────────────────┐  │
│  │                                                        │  │
│  │                                                        │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                              │
├──────────────────────────────────────────────────────────────┤
│                        [ Cancel ]      [ Mark resolved ]     │
└──────────────────────────────────────────────────────────────┘
```

`ResolveDefectRequest` carries only `resolution` — max 500. The resolving actor and the timestamp are the
server's to set, and the dialog must not offer either.

**No confirmation step.** The typed resolution is the confirmation; a second "are you sure?" over a
reversible-by-nature record is noise.

### The two 409s

Both are ordinary, and both are the reason this dialog needs a real error area rather than a toast.

```text
┌──────────────────────────────────────────────────────────────┐
│  ⚠ This defect was already resolved by someone else.         │
│    [ Reload ]                                                │
└──────────────────────────────────────────────────────────────┘
```

`Defects.AlreadyResolved` — someone got there first. **Reload** closes the dialog and refetches the detail,
which will now render its resolved footer.

`Concurrency.Conflict` — the record changed under the request (`xmin` moved). Same banner, same **Reload**.
The user's typed resolution stays in the box until they choose to discard it; losing a paragraph of typing
to someone else's click is exactly the kind of small betrayal that makes people distrust a tool.

---

## 5. Field reference

Everything the four endpoints exchange, so no screen has to guess.

**`DefectListItem`** — `defectId`, `stationCode`, `errorCode`, `severity`, `description`, `raisedBy`,
`createdAt`, `resolvedAt?`

**`DefectDetailResponse`** — `defectId`, `stationId`, `stationCode`, `stationName`, `errorCodeId`,
`errorCode`, `errorCodeDescription`, `severity`, `description`, `raisedBy`, `createdAt`, `resolution?`,
`resolvedBy?`, `resolvedAt?`

**`CreateDefectRequest`** — `stationCode`, `errorCode`, `description` → **`CreateDefectResponse`**
`{ defectId }`

**`ResolveDefectRequest`** — `resolution` → no body

**Limits** — description 500, resolution 500, station code 50, error code 50.
