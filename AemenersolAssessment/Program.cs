using Microsoft.EntityFrameworkCore;
using AemenersolAssessment.Data;
using AemenersolAssessment.Services;

var baseUrl = "http://test-demo.aemenersol.com";
var username = "user@aemenersol.com";
var password = "Test@123";

Console.WriteLine("=== PlatformWell Data Sync ===\n");

// Ensure database exists (Code First)
using (var db = new AppDbContext())
{
    if (db.Database.IsSqlite())
        await db.Database.EnsureCreatedAsync();
    else
        await db.Database.MigrateAsync();
    Console.WriteLine("Database ready.\n");
}

// Login
var api = new ApiClient(baseUrl);
Console.WriteLine("Logging in...");
await api.LoginAsync(username, password);
Console.WriteLine("Login successful.\n");

// Sync from GetPlatformWellActual
Console.WriteLine("Fetching GetPlatformWellActual...");
var actualData = await api.GetPlatformWellAsync("/api/PlatformWell/GetPlatformWellActual");
Console.WriteLine($"Received {actualData.Count} platforms.\n");

var syncService = new DataSyncService();
int count = await syncService.SyncAsync(actualData);
Console.WriteLine($"Upserted {count} records (platforms + wells).\n");

// Verify by calling GetPlatformWellDummy (tests missing/extra keys handling)
Console.WriteLine("Fetching GetPlatformWellDummy...");
var dummyData = await api.GetPlatformWellAsync("/api/PlatformWell/GetPlatformWellDummy");
Console.WriteLine($"Received {dummyData.Count} platforms.\n");

count = await syncService.SyncAsync(dummyData);
Console.WriteLine($"Upserted {count} records from dummy data (handled missing keys gracefully).\n");

// Show final state
using (var db = new AppDbContext())
{
    var platforms = await db.Platforms
        .Include(p => p.Wells)
        .OrderBy(p => p.Id)
        .ToListAsync();

    Console.WriteLine($"Final state: {platforms.Count} platforms, {platforms.Sum(p => p.Wells.Count)} wells.\n");

    // Run the Part 2 query: last updated well for each platform
    Console.WriteLine("=== Last Updated Well Per Platform ===\n");
    var result = await db.Wells
        .Where(w => w.Id == db.Wells
            .Where(w2 => w2.PlatformId == w.PlatformId)
            .OrderByDescending(w2 => w2.UpdatedAt)
            .ThenByDescending(w2 => w2.Id)
            .Select(w2 => w2.Id)
            .FirstOrDefault())
        .OrderBy(w => w.PlatformId)
        .ToListAsync();

    var platformNames = await db.Platforms
        .ToDictionaryAsync(p => p.Id, p => p.UniqueName);

    Console.WriteLine("PlatformName | WellId | PlatformId | UniqueName | Latitude | Longitude | CreatedAt | UpdatedAt");
    Console.WriteLine(new string('-', 120));
    foreach (var w in result)
    {
        var pname = platformNames.GetValueOrDefault(w.PlatformId, "?");
        Console.WriteLine($"{pname} | {w.Id} | {w.PlatformId} | {w.UniqueName} | {w.Latitude} | {w.Longitude} | {w.CreatedAt:yyyy-MM-dd HH:mm:ss} | {w.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
    }
}

Console.WriteLine("\nDone.");
