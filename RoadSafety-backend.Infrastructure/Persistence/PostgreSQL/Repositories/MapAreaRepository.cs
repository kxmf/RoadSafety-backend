using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Npgsql;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Context;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Repositories;

public sealed class MapAreaRepository(ApplicationDbContext dbContext) : IMapAreaRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<byte[]> GetVectorTileAsync(string cityId, int z, int x, int y, CancellationToken cancellationToken)
    {
        var connection = _dbContext.Database.GetDbConnection();
        await using var command = connection.CreateCommand();

        command.CommandText = """
            WITH bounds AS (
                SELECT
                    ST_TileEnvelope(@z, @x, @y) AS geom_3857,
                    ST_Transform(ST_TileEnvelope(@z, @x, @y), 4326) AS geom_4326
            ),
            mvtgeom AS (
                SELECT
                    base_area_key AS "baseAreaKey",
                    lower(risk) AS risk,
                    ST_AsMVTGeom(
                        ST_SimplifyPreserveTopology(
                            ST_Transform(map_areas.geom, 3857),
                            CASE
                                WHEN @z <= 12 THEN 20.0
                                WHEN @z = 13 THEN 10.0
                                WHEN @z = 14 THEN 5.0
                                WHEN @z = 15 THEN 2.0
                                ELSE 0.5
                            END),
                        bounds.geom_3857,
                        4096,
                        64,
                        true) AS geom
                FROM map_areas, bounds
                WHERE city_id = @cityId
                  AND geom && bounds.geom_4326
                  AND ST_Intersects(geom, bounds.geom_4326)
            )
            SELECT COALESCE(ST_AsMVT(mvtgeom, 'safety_zones', 4096, 'geom'), '\x'::bytea)
            FROM mvtgeom
            WHERE geom IS NOT NULL;
            """;

        command.Parameters.Add(new NpgsqlParameter<int>("z", z));
        command.Parameters.Add(new NpgsqlParameter<int>("x", x));
        command.Parameters.Add(new NpgsqlParameter<int>("y", y));
        command.Parameters.Add(new NpgsqlParameter<string>("cityId", cityId));

        var shouldCloseConnection = connection.State != System.Data.ConnectionState.Open;
        if (shouldCloseConnection)
            await connection.OpenAsync(cancellationToken);

        try
        {
            var result = await command.ExecuteScalarAsync(cancellationToken);

            return result is byte[] bytes ? bytes : [];
        }
        finally
        {
            if (shouldCloseConnection)
                await connection.CloseAsync();
        }
    }

    public async Task<List<MapArea>> GetByCityAsync(string cityId, CancellationToken cancellationToken)
    {
        return await _dbContext.MapAreas
            .AsNoTracking()
            .Where(area => area.CityId == cityId)
            .ToListAsync(cancellationToken);
    }

    public async Task<MapCityMetadata?> GetCityMetadataAsync(string cityId, CancellationToken cancellationToken)
    {
        return await _dbContext.MapCityMetadata
            .AsNoTracking()
            .FirstOrDefaultAsync(metadata => metadata.CityId == cityId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(MapAreaId id, CancellationToken cancellationToken)
    {
        return await _dbContext.MapAreas.AnyAsync(area => area.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByBaseAreaKeyAsync(string baseAreaKey, CancellationToken cancellationToken)
    {
        return await _dbContext.MapAreas.AnyAsync(area => area.BaseAreaKey == baseAreaKey, cancellationToken);
    }

    public async Task ReplaceCityAreasAsync(string cityId, IReadOnlyCollection<MapArea> areas, CancellationToken cancellationToken)
    {
        var existingAreas = await _dbContext.MapAreas
            .Where(area => area.CityId == cityId)
            .ToListAsync(cancellationToken);

        _dbContext.MapAreas.RemoveRange(existingAreas);
        await _dbContext.MapAreas.AddRangeAsync(areas, cancellationToken);
    }

    public async Task UpsertCityMetadataAsync(
        string cityId,
        DateTimeOffset generationVersion,
        double minLon,
        double minLat,
        double maxLon,
        double maxLat,
        CancellationToken cancellationToken)
    {
        var metadata = await _dbContext.MapCityMetadata
            .FirstOrDefaultAsync(item => item.CityId == cityId, cancellationToken);

        if (metadata is null)
        {
            await _dbContext.MapCityMetadata.AddAsync(
                MapCityMetadata.Create(cityId, generationVersion, minLon, minLat, maxLon, maxLat),
                cancellationToken);

            return;
        }

        metadata.Update(generationVersion, minLon, minLat, maxLon, maxLat);
    }
}
