using Dapper;
using VehicleService.Data.Entities;
using VehicleService.Data.Interfaces;

namespace VehicleService.Data.Implementations;

public class VehicleRepository : IVehicleRepository
{
    private readonly DatabaseConnectionFactory _connectionFactory;

    public VehicleRepository(DatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<VehicleEntity>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<VehicleEntity>("SELECT id, make, model, year, vin, license_plate as LicensePlate FROM vehicles");
    }

    public async Task<VehicleEntity?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<VehicleEntity>(
            "SELECT id, make, model, year, vin, license_plate as LicensePlate FROM vehicles WHERE id = @Id",
            new { Id = id });
    }

    public async Task<int> CreateAsync(VehicleEntity vehicleEntity)
    {
        using var connection = _connectionFactory.CreateConnection();
        var query = """
                    
                                INSERT INTO vehicles (make, model, year, vin, license_plate)
                                VALUES (@Make, @Model, @Year, @Vin, @LicensePlate)
                                RETURNING id;
                            
                    """;
        return await connection.ExecuteScalarAsync<int>(query, vehicleEntity);
    }

    public async Task<bool> UpdateAsync(VehicleEntity vehicleEntity)
    {
        using var connection = _connectionFactory.CreateConnection();
        var query = """
                    
                                UPDATE vehicles 
                                SET make = @Make, model = @Model, year = @Year, vin = @Vin, license_plate = @LicensePlate
                                WHERE id = @Id;
                            
                    """;
        return await connection.ExecuteAsync(query, vehicleEntity) > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync("DELETE FROM vehicles WHERE id = @Id", new { Id = id }) > 0;
    }
}