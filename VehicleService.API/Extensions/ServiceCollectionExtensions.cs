using FluentMigrator.Runner;
using Microsoft.OpenApi.Models;
using VehicleService.Data;
using VehicleService.Data.Implementations;
using VehicleService.Data.Interfaces;
using VehicleService.Data.Migrations;

namespace VehicleService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(o =>
        {
            o.SwaggerDoc("v1", new OpenApiInfo { Title = "VehicleService", Version = "v1" });
            o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please insert JWT without Bearer into field",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
            });
            o.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    []
                }
            });
        });
        return builder;
    }

    public static WebApplicationBuilder AddMigrator(this WebApplicationBuilder builder)
    {
        builder.Services.AddFluentMigratorCore()
            .ConfigureRunner(rb =>
                rb.AddPostgres()
                    .WithGlobalConnectionString(builder.Configuration.GetConnectionString("DefaultConnection"))
                    .ScanIn(typeof(InitializeMigrations).Assembly).For.Migrations()
            );
        return builder;
    }
    
    public static WebApplicationBuilder AddData(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<DatabaseConnectionFactory>();
        builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
        return builder;
    }
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        return builder;
    }
}