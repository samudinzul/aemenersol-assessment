# PlatformWell Sync — .NET Technical Assessment

A .NET 8 console application that logs into the test Web API, pulls platform/well
data, and upserts it into a SQL Server LocalDB database using Entity Framework
Core (Code First).

## Requirements covered

| # | Requirement | Where |
|---|-------------|-------|
| 3 | Login to the Web API | `PlatformWellSync/Services/ApiClient.cs` (`LoginAsync`) |
| 4 | Use returned bearer token to call `GetPlatformWellActual` | `PlatformWellSync/Services/ApiClient.cs`, `PlatformWellSync/Program.cs` |
| 5 | Store platform data in `Platform` table, well data in `Well` table | `PlatformWellSync/Models/Platform.cs`, `PlatformWellSync/Models/Well.cs` |
| 6 | Upsert by `id` (update if exists, insert otherwise) | `PlatformWellSync/Services/DataSyncService.cs` |
| 7 | Don't break on missing/extra keys (tested with `GetPlatformWellDummy`) | `PlatformWellSync/Models/Dtos.cs`, `PlatformWellSync/Services/DataSyncService.cs` |
| b | Entity Framework Code First | `PlatformWellSync/Data/AppDbContext.cs`, `PlatformWellSync/Migrations/` |
| Part 2 | SQL query: last updated well per platform | `PlatformWellSync/Queries/LastUpdatedWellPerPlatform.sql` |

## How requirement 7 is handled

`GetPlatformWellActual` returns `createdAt` / `updatedAt`, while
`GetPlatformWellDummy` returns a different shape (`lastUpdate` instead of
`createdAt`/`updatedAt`, and no `createdAt` at all).

- The DTOs declare all known keys as **nullable**, so a missing key simply maps to `null`.
- `System.Text.Json` ignores **unknown/new** keys by default, so added keys don't break deserialization.
- In `DataSyncService`, `UpdatedAt = dto.UpdatedAt ?? dto.LastUpdate ?? existing`, so the app
  keeps working with either payload shape while only handling keys from the original dataset.

## Prerequisites

- .NET 8 SDK
- SQL Server LocalDB (installed with Visual Studio, or the standalone LocalDB installer)

## Run

```bash
cd PlatformWellSync
dotnet run
```

By default the app uses this connection string:

```
Server=(localdb)\mssqllocaldb;Database=PlatformWellDb;Trusted_Connection=True;TrustServerCertificate=True;
```

The schema is created from the EF Core migration on first run.

### Using a different database

The provider defaults to **SQL Server**. Override with environment variables:

- `CONNECTION_STRING` — the connection string (defaults to the LocalDB one above).
- `DB_PROVIDER` — `SqlServer` (default) or `Sqlite`.

When `DB_PROVIDER=Sqlite` the app uses `EnsureCreated`; otherwise it applies the
EF Core migration.

```bash
# SQLite (cross-platform smoke test)
DB_PROVIDER=Sqlite CONNECTION_STRING="Data Source=platformwell.db" dotnet run
```

The same solution can be pointed at a full SQL Server (e.g. in Docker) to test the
real migration and the Part 2 query under SQL Server semantics:

```bash
docker run -d --name mssql -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD=Your_password123 \
  -e MSSQL_PID=Developer -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest

CONNECTION_STRING="Server=localhost,1433;Database=PlatformWellDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;" dotnet run
```

## Project layout

```
PlatformWellSync/
├── Data/AppDbContext.cs            # EF Core DbContext (Code First)
├── Migrations/                     # EF Core migration for SQL Server
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

## Part 2 — SQL query

See [`Queries/LastUpdatedWellPerPlatform.sql`](PlatformWellSync/Queries/LastUpdatedWellPerPlatform.sql).
It joins `Platform` and `Well` and picks, per platform, the well with the greatest
`UpdatedAt` (ties broken by `Id`), returning the columns shown in the expected result.

## Time spent

~{N} hours — see `TIME_SPENT.md`.
