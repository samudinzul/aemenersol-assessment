using Microsoft.EntityFrameworkCore;
using AemenersolAssessment.Data;
using AemenersolAssessment.Models;

namespace AemenersolAssessment.Services;

public class DataSyncService
{
    public async Task<int> SyncAsync(List<PlatformDto> platforms)
    {
        using var db = new AppDbContext();
        int platformsUpserted = 0, wellsUpserted = 0;

        foreach (var pdto in platforms)
        {
            var platform = await db.Platforms
                .Include(p => p.Wells)
                .FirstOrDefaultAsync(p => p.Id == pdto.Id);

            if (platform == null)
            {
                platform = new Platform { Id = pdto.Id };
                db.Platforms.Add(platform);
            }

            platform.UniqueName = pdto.UniqueName ?? platform.UniqueName;
            platform.Latitude = pdto.Latitude;
            platform.Longitude = pdto.Longitude;
            platform.CreatedAt = pdto.CreatedAt ?? platform.CreatedAt;
            platform.UpdatedAt = pdto.UpdatedAt ?? pdto.LastUpdate ?? platform.UpdatedAt;
            platformsUpserted++;

            if (pdto.Wells != null)
            {
                foreach (var wdto in pdto.Wells)
                {
                    var well = platform.Wells.FirstOrDefault(w => w.Id == wdto.Id);

                    if (well == null)
                    {
                        well = new Well { Id = wdto.Id, PlatformId = pdto.Id };
                        platform.Wells.Add(well);
                    }

                    well.UniqueName = wdto.UniqueName ?? well.UniqueName;
                    well.Latitude = wdto.Latitude;
                    well.Longitude = wdto.Longitude;
                    well.CreatedAt = wdto.CreatedAt ?? well.CreatedAt;
                    well.UpdatedAt = wdto.UpdatedAt ?? wdto.LastUpdate ?? well.UpdatedAt;
                    wellsUpserted++;
                }
            }
        }

        await db.SaveChangesAsync();
        return platformsUpserted + wellsUpserted;
    }
}
