using FluentMigrator;

namespace VehicleService.Data.Migrations;

[Migration(20250211)]
public class InitializeMigrations: Migration
{
    public override void Up()
    {
        Execute.Sql("""
                    CREATE TABLE users (
                                           id SERIAL PRIMARY KEY,
                                           name VARCHAR(255) NOT NULL,
                                           email VARCHAR(255) UNIQUE NOT NULL,
                                           password_hash TEXT NOT NULL,
                                           role VARCHAR(50) CHECK (role IN ('admin', 'dispatcher', 'driver')) NOT NULL,
                                           created_at TIMESTAMP DEFAULT NOW()
                    );
                    """);
        Execute.Sql("""
                    CREATE TABLE vehicles (
                                              id SERIAL PRIMARY KEY,
                                              make VARCHAR(100) NOT NULL,
                                              model VARCHAR(100) NOT NULL,
                                              year INT NOT NULL,
                                              vin VARCHAR(17) UNIQUE NOT NULL,
                                              license_plate VARCHAR(20) UNIQUE NOT NULL,
                                              mileage INT DEFAULT 0,
                                              fuel_type VARCHAR(50) CHECK (fuel_type IN ('Gasoline', 'Diesel', 'Electric', 'Hybrid')),
                                              owner_id INT REFERENCES users(id) ON DELETE SET NULL
                    );
                    """);
        Execute.Sql("""
                    CREATE TABLE reports (
                                             id SERIAL PRIMARY KEY,
                                             vehicle_id INT REFERENCES vehicles(id) ON DELETE CASCADE,
                                             driver_id INT REFERENCES users(id) ON DELETE SET NULL,
                                             date DATE NOT NULL,
                                             start_mileage INT NOT NULL,
                                             end_mileage INT NOT NULL,
                                             fuel_consumed FLOAT,
                                             route TEXT,
                                             notes TEXT
                    );
                    """);
        Execute.Sql("""
                    CREATE TABLE fuel_logs (
                                               id SERIAL PRIMARY KEY,
                                               vehicle_id INT REFERENCES vehicles(id) ON DELETE CASCADE,
                                               driver_id INT REFERENCES users(id) ON DELETE SET NULL,
                                               date DATE NOT NULL,
                                               fuel_type VARCHAR(50) CHECK (fuel_type IN ('Gasoline', 'Diesel', 'Electric', 'Hybrid')),
                                               liters FLOAT NOT NULL,
                                               cost DECIMAL(10,2) NOT NULL
                    );
                    """);
        Execute.Sql("""
                    CREATE TABLE repairs (
                                             id SERIAL PRIMARY KEY,
                                             vehicle_id INT REFERENCES vehicles(id) ON DELETE CASCADE,
                                             date DATE NOT NULL,
                                             description TEXT NOT NULL,
                                             cost DECIMAL(10,2) NOT NULL
                    );
                    """);
    }

    public override void Down()
    {
        Delete.Table("repairs");
        Delete.Table("fuel_logs");
        Delete.Table("reports");
        Delete.Table("vehicles");
        Delete.Table("users");
    }
}