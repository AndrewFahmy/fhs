# FHS Claude Code Contract

## Authority

- Treat `docs/chain-composition.md` as the canonical architecture document.
- Read its relevant sections and a comparable existing feature before proposing implementation work.
- Surface undecided or deferred areas instead of inventing conventions.

## Manual Development

- The developer writes all application code manually.
- Do not create, edit, move, delete, or generate files under `backend/`, `infra/`, `frontend/`, or `tests/`.
- Do not create, edit, move, delete, or generate root build and solution files.
- You may update `docs/**` only after the developer explicitly confirms the implemented behavior or design decision.
- Deliver code as ordered, manual implementation steps with exact destination files and self-contained snippets.
- Provide one implementation step at a time and wait for the developer to report completion or an issue before continuing.

## Verification

- You may inspect repository and environment information before recommending a step.
- Preserve the project agreement to write all planned feature steps before building or testing.
- Once the developer has completed the planned steps, recommend the smallest relevant build and test commands.
- Do not claim implementation is complete until the developer reports every presented step complete and final verification results are known.

## Chain Composition

- An endpoint declaration must read as the feature's ordered story.
- Input validation is the first link.
- `SaveChanges` is terminal.
- Declare State hand-offs between links with `Requires` and `Produces` attributes.
- Keep capability interfaces narrow and link dependencies constructor-injected.
- Respect no-tracking reads with explicit writes.
