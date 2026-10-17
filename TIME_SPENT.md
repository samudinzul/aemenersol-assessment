# Time Spent

Total: **~5 hours**

## Breakdown

| Activity | Time |
|----------|------|
| Reviewing the API (Swagger, login, sample responses for `Actual` and `Dummy`) | 0.5 h |
| Setting up the project, EF Core, Code First entities + migration | 1 h |
| Login / bearer-token client and API calls | 0.75 h |
| Upsert (update-or-insert) sync logic | 1 h |
| Handling missing/extra keys (`GetPlatformWellDummy`) | 0.75 h |
| Part 2 SQL query | 0.5 h |
| Testing, cleanup and documentation | 0.5 h |

## Why this time was required

- **API review** — I inspected the Swagger definition and the live responses for
  both `GetPlatformWellActual` and `GetPlatformWellDummy` to understand the data
  model and, importantly, the differences between the two payloads.
- **Model / EF Core setup** — I designed the `platform` and `well` tables, set the
  foreign key, and used Code First with a migration rather than a hand-written
  schema. Ids come from the API, so they are configured as explicit values (not
  identity columns), which needed care to avoid SQL Server insert errors.
- **Upsert logic** — the core of the exercise. Each record is looked up by `id`;
  it is updated if present and inserted if not. Running the sync twice had to leave
  the data unchanged (no duplicates), which I verified.
- **Missing/extra keys** — the `Dummy` endpoint drops `createdAt`/`updatedAt` and
  adds `lastUpdate`. I made the DTOs tolerant (nullable properties, unknown keys
  ignored) and mapped `updatedAt ?? lastUpdate`, so the same sync code handles both
  shapes without breaking.
- **Part 2 query** — writing and validating the query that returns the last updated
  well per platform, including the tie-break on `Id`, and matching the expected
  result columns exactly.
- **Testing** — end-to-end runs against SQL Server LocalDB and a cross-platform
  SQLite run to confirm behaviour, plus writing the README so the solution is easy
  to run and review.
