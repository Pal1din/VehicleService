using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace VehicleService.Data;

public class DatabaseConnectionFactory(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new Exception("Connection string not found");

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}