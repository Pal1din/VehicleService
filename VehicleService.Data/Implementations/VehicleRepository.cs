using Dapper;
using VehicleService.Data.Entities;
using VehicleService.Data.Interfaces;

namespace VehicleService.Data.Implementations;

public class VehicleRepository(DatabaseConnectionFactory connectionFactory, RedisCacheService cacheService)
    : IVehicleRepository
{
    private const string CachePrefix = "vehicle:";

    public async Task<IEnumerable<VehicleEntity>> GetAllAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryAsync<VehicleEntity>(
            "SELECT id, make, model, year, vin, license_plate as LicensePlate FROM vehicles");
    }

    public async Task<VehicleEntity?> GetByIdAsync(int id)
    {
        var cacheKey = $"{CachePrefix}{id}";
        var cachedVehicle = await cacheService.GetCacheAsync<VehicleEntity?>(cacheKey);
        if (cachedVehicle != null) return cachedVehicle;

        using var connection = connectionFactory.CreateConnection();
        var vehicle = await connection.QuerySingleOrDefaultAsync<VehicleEntity>(
            "SELECT id, make, model, year, vin, license_plate as LicensePlate FROM vehicles WHERE id = @Id",
            new { Id = id });

        if (vehicle != null)
            await cacheService.SetCacheAsync(cacheKey, vehicle, TimeSpan.FromMinutes(10));

        return vehicle;
    }

    public async Task<int> CreateAsync(VehicleEntity vehicleEntity)
    {
        using var connection = connectionFactory.CreateConnection();
        var query = """
                            INSERT INTO vehicles (make, model, year, vin, license_plate)
                            VALUES (@Make, @Model, @Year, @Vin, @LicensePlate)
                            RETURNING id;
                    """;
        return await connection.ExecuteScalarAsync<int>(query, vehicleEntity);
    }

    public async Task<bool> UpdateAsync(VehicleEntity vehicleEntity)
    {
        using var connection = connectionFactory.CreateConnection();
        var query = """
                            UPDATE vehicles 
                            SET make = @Make, model = @Model, year = @Year, vin = @Vin, license_plate = @LicensePlate
                            WHERE id = @Id;
                    """;
        var result = await connection.ExecuteAsync(query, vehicleEntity) > 0;
        if (result)
            await cacheService.RemoveCacheAsync($"{CachePrefix}{vehicleEntity.Id}");
        return result;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync("DELETE FROM vehicles WHERE id = @Id", new { Id = id }) > 0;
    }
}