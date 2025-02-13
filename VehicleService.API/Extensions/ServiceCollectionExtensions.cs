using FluentMigrator.Runner;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
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
        builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));
        builder.Services.AddSingleton<RedisCacheService>();
        return builder;
    }
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        return builder;
    }

    internal static WebApplicationBuilder AddAuthService(this WebApplicationBuilder builder, AuthenticationSettings authSettings)
    {
        builder.Services.AddAuthentication(authSettings.TokenOptions.Scheme)
            .AddJwtBearer(authSettings.TokenOptions.Scheme, options =>
            {
                options.Authority = authSettings.TokenOptions.Authority;
                options.Audience = authSettings.TokenOptions.Audience;
                options.RequireHttpsMetadata = authSettings.TokenOptions.RequireHttpsMetadata;
            });
        builder.Services.AddAuthorization(options =>
        {
            foreach (var policyOptions in authSettings.Policies)
            {
                options.AddPolicy(policyOptions.Name, policy => policy.RequireAssertion(context => 
                    policyOptions.Claims.TrueForAll(c => context.User.HasClaim(c.Type, c.Value))));
            }
            
            // options.AddPolicy("CanWrite", policy => policy.RequireAssertion(context => 
            //     context.User.HasClaim("scope", "write") && context.User.HasClaim("permission", "write")));
        });
        return builder;
    }
}