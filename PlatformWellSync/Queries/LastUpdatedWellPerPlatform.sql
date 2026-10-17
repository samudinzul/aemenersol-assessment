-- PART 2: Technical Assessment (SQL Query)
-- Return the last updated well for each platform.
--
-- The correlated subquery finds the well with the greatest UpdatedAt within
-- each platform; ties are broken by Well Id to guarantee a single row.
SELECT
    p.UniqueName AS PlatformName,
    w.Id,
    w.PlatformId,
    w.UniqueName,
    w.Latitude,
    w.Longitude,
    w.CreatedAt,
    w.UpdatedAt
FROM Platform p
INNER JOIN Well w
    ON w.PlatformId = p.Id
WHERE w.Id = (
    SELECT TOP 1 w2.Id
    FROM Well w2
    WHERE w2.PlatformId = p.Id
    ORDER BY w2.UpdatedAt DESC, w2.Id DESC
)
ORDER BY p.UniqueName;

-- Equivalent query using a ranking window function:
--
-- SELECT PlatformName, Id, PlatformId, UniqueName, Latitude, Longitude, CreatedAt, UpdatedAt
-- FROM (
--     SELECT
--         p.UniqueName AS PlatformName,
--         w.Id, w.PlatformId, w.UniqueName, w.Latitude, w.Longitude, w.CreatedAt, w.UpdatedAt,
--         ROW_NUMBER() OVER (PARTITION BY w.PlatformId ORDER BY w.UpdatedAt DESC, w.Id DESC) AS rn
--     FROM Platform p
--     INNER JOIN Well w ON w.PlatformId = p.Id
-- ) ranked
-- WHERE rn = 1
-- ORDER BY PlatformName;
