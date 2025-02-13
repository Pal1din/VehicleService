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

            o.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Scheme = "oauth2",
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri("https://localhost:7138/connect/authorize"),
                        TokenUrl = new Uri("https://localhost:7138/connect/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            {"read", "Доступ на чтение"},
                            {"write", "Доступ на запись"}
                        }
                    }
                }
            });
            o.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        }
                    }, ["read", "write"]
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

    public static WebApplicationBuilder AddAuthService(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = "https://localhost:7138";
                options.Audience = "vehicle.resource";
                options.RequireHttpsMetadata = false;
            });
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("read", policyBuilder => policyBuilder.RequireClaim("scope", "read"));
            options.AddPolicy("write", policyBuilder => policyBuilder.RequireClaim("scope", "write"));
        });
        return builder;
    }
}