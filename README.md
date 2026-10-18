# AemenersolAssessment — .NET Technical Assessment

## PART 1 — TECHNICAL ASSESSMENT (.NET APPLICATION)

## Prerequisites

| Tool | Notes |
|------|-------|
| **.NET 8 SDK** | `dotnet --list-sdks` should show an `8.x` SDK |
| **SQL Server LocalDB** | Ships with Visual Studio / SQL Server Express. Verify with `sqllocaldb info` (should print `MSSQLLOCALDB`) |

> No LocalDB? See [Running without LocalDB](#running-without-localdb-sqlite) — the
> app also runs on SQLite for a quick cross-platform check.

---

Build a .NET Core application that logs into the Web API, uses the returned bearer
token to call `GetPlatformWellActual`, and upserts the platform/well data into a SQL
Server LocalDB database using EF Core Code First.

| # | Requirement | Where |
|---|-------------|-------|
| 3 | Login to the Web API | `AemenersolAssessment/Services/ApiClient.cs` (`LoginAsync`) |
| 4 | Use bearer token to call `GetPlatformWellActual` | `Services/ApiClient.cs`, `Program.cs` |
| 5 | Store platform data in `platform` table, well data in `well` table | `Models/Platform.cs`, `Models/Well.cs` |
| 6 | Upsert by `id` (update if exists, insert otherwise) | `Services/DataSyncService.cs` |
| 7 | Don't break on missing/extra keys (tested with `GetPlatformWellDummy`) | `Models/Dtos.cs`, `Services/DataSyncService.cs` |
| b | Entity Framework Code First | `Data/AppDbContext.cs`, `Migrations/` |

## How to run

```bash
cd AemenersolAssessment
dotnet run
```

On the first run the app:
- restores NuGet packages,
- creates the `PlatformWellDb` database from the EF Core migration (`(localdb)\mssqllocaldb`),
- logs in, syncs `GetPlatformWellActual`, then re-syncs `GetPlatformWellDummy`,
- prints the Part 2 result.

## Expected output (Part 1)

```
=== PlatformWell Data Sync ===

Database ready.

Logging in...
Login successful.

Fetching GetPlatformWellActual...
Received 10 platforms.

Upserted 25 records (platforms + wells).

Fetching GetPlatformWellDummy...
Received 10 platforms.

Upserted 25 records from dummy data (handled missing keys gracefully).

Final state: 10 platforms, 15 wells.
```

### Verify the upsert (optional)

Run `dotnet run` a second time. It stays at **10 platforms, 15 wells** — no duplicates
— proving the update-or-insert logic.

### Inspect the data (optional)

Connect to `(localdb)\mssqllocaldb` in SSMS / SQL Server Object Explorer →
`PlatformWellDb` → tables `platform` and `well`.

## Requirement 7 — missing / extra keys

`GetPlatformWellActual` returns `createdAt` / `updatedAt`, while
`GetPlatformWellDummy` returns a different shape (`lastUpdate` instead of
`createdAt`/`updatedAt`, and no `createdAt` at all).

- DTOs declare all known keys as **nullable**, so a missing key maps to `null`.
- `System.Text.Json` ignores **unknown/new** keys by default, so added keys don't
  break deserialization.
- In `DataSyncService`: `UpdatedAt = dto.UpdatedAt ?? dto.LastUpdate ?? existing`,
  so the app works with either payload while only handling the original keys.

---

# PART 2 — TECHNICAL ASSESSMENT (SQL QUERY)

**Task:** based on the `platform` and `well` tables from Part 1, write a SQL query
that returns the **last updated well for each platform**.

File: [`AemenersolAssessment/Queries/LastUpdatedWellPerPlatform.sql`](AemenersolAssessment/Queries/LastUpdatedWellPerPlatform.sql)

```sql
SELECT
    p.UniqueName AS PlatformName,
    w.Id,
    w.PlatformId,
    w.UniqueName,
    w.Latitude,
    w.Longitude,
    w.CreatedAt,
    w.UpdatedAt
FROM platform p
INNER JOIN well w
    ON w.PlatformId = p.Id
WHERE w.Id = (
    SELECT TOP 1 w2.Id
    FROM well w2
    WHERE w2.PlatformId = p.Id
    ORDER BY w2.UpdatedAt DESC, w2.Id DESC
)
ORDER BY p.UniqueName;
```

For each platform, the correlated subquery picks the single most recently updated
well. `Id DESC` is a tie-breaker so equal `UpdatedAt` values can't return two rows.
The file also contains an equivalent `ROW_NUMBER()` window-function version.

## Expected output (Part 2)

```
PlatformName | Id | PlatformId | UniqueName | Latitude  | Longitude  | CreatedAt           | UpdatedAt
--------------------------------------------------------------------------------------------------------------
Platform1    | 1  | 11         | Well11     | 37.06257  | 18.406885  | 2017-11-01 02:41:00 | 2018-08-04 02:16:42
Platform2    | 5  | 12         | Well22     | 181.01989 | 92.034426  | 2018-11-24 06:12:26 | 2018-08-20 14:59:48
Platform3    | 8  | 13         | Well32     | 289.631824| 147.255081 | 2015-11-18 23:08:44 | 2016-10-20 12:20:45
Platform4    | 10 | 14         | Well41     | 357.894286| 184.068851 | 2017-04-16 09:55:14 | 2018-09-16 19:26:11
Platform5    | 14 | 15         | Well52     | 489.834418| 257.696391 | 2017-10-29 07:48:12 | 2016-01-28 18:30:25
```

This matches the expected result in the assessment.

## How to run the query

In SSMS against `PlatformWellDb`, or from a command line:

```powershell
sqlcmd -S "(localdb)\mssqllocaldb" -d PlatformWellDb -i AemenersolAssessment\Queries\LastUpdatedWellPerPlatform.sql
```

---

## Running without LocalDB (SQLite)

Set the provider via environment variables:

```bash
DB_PROVIDER=Sqlite CONNECTION_STRING="Data Source=platformwell.db" dotnet run
```

PowerShell:

```powershell
$env:DB_PROVIDER="Sqlite"; $env:CONNECTION_STRING="Data Source=platformwell.db"; dotnet run
```

When `DB_PROVIDER=Sqlite` the app creates the schema with `EnsureCreated` instead of
migrations. Everything else (login, sync, upsert, Part 2 output) behaves the same.

### Point at a real SQL Server (e.g. Docker)

```bash
docker run -d --name mssql -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD=Your_password123 \
  -e MSSQL_PID=Developer -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest

CONNECTION_STRING="Server=localhost,1433;Database=PlatformWellDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;" dotnet run
```

---

## Configuration

| Variable | Default | Purpose |
|----------|---------|---------|
| `CONNECTION_STRING` | `Server=(localdb)\mssqllocaldb;Database=PlatformWellDb;Trusted_Connection=True;TrustServerCertificate=True;` | Database connection |
| `DB_PROVIDER` | `SqlServer` | `SqlServer` or `Sqlite` |

---

## Project layout

```
AemenersolAssessment/
├── Data/AppDbContext.cs            # EF Core DbContext (Code First)
├── Migrations/                     # EF Core migration (platform / well tables)
├── Models/
│   ├── Platform.cs                 # Platform entity
│   ├── Well.cs                     # Well entity
│   └── Dtos.cs                     # API request/response DTOs
├── Queries/
│   └── LastUpdatedWellPerPlatform.sql   # Part 2 query
├── Services/
│   ├── ApiClient.cs                # Login + API calls
│   └── DataSyncService.cs          # Upsert logic
└── Program.cs                      # Entry point / demo
```

---

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| `No .NET SDKs were found` | Install the .NET 8 SDK, open a new terminal |
| `Unable to locate a Local Database Runtime installation` | Install SQL Server LocalDB, or use the SQLite mode above |
| `dotnet` not recognised after LocalDB/MSI install | Open a new PowerShell window |
| API login/sync errors | Check internet access to `test-demo.aemenersol.com` |

---

## Time spent

~5 hours — see `TIME_SPENT.md` for the breakdown and reasoning.
