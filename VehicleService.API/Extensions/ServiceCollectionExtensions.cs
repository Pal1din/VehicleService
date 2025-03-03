using FluentMigrator.Runner;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using StackExchange.Redis;
using VehicleService.Data;
using VehicleService.Data.Implementations;
using VehicleService.Data.Interfaces;
using VehicleService.Data.Migrations;

namespace VehicleService.API.Extensions;

public static class ServiceCollectionExtensions
{
    internal static WebApplicationBuilder AddTelemetryMetrics(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("VehicleService")
                .SetSampler(new AlwaysOnSampler()))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddPrometheusExporter());
        return builder;
    }

    internal static WebApplicationBuilder ConfigureSerilog(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateBootstrapLogger();
        builder.Host.UseSerilog();
        return builder;
    }
    internal static WebApplicationBuilder AddDefault(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer()
            .AddHttpContextAccessor()
            .AddControllers();
        return builder;
    }

    internal static WebApplicationBuilder AddApplicationCors(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowLocalhost5173", policy =>
            {
                policy.WithOrigins("http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
        return builder;
    }
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
                        AuthorizationUrl = new Uri("https://45.91.238.163:8080/connect/authorize"),
                        TokenUrl = new Uri("https://45.91.238.163:8080/connect/token"),
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
                        },
                        Type = SecuritySchemeType.OAuth2,
                        Scheme = "oauth2",
                        
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
    
    internal static WebApplicationBuilder AddDbContext(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<DatabaseConnectionFactory>();
        builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));
        builder.Services.AddSingleton<RedisCacheService>();
        return builder;
    }

    internal static WebApplicationBuilder AddRepositories(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
        return builder;
    }
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        return builder;
    }

    internal static WebApplicationBuilder AddAuthService(this WebApplicationBuilder builder, IConfigurationSection section)
    {
        var authSettings = section.Get<AuthenticationSettings>()!;
        builder.Services.AddAuthentication(authSettings.TokenOptions.Scheme)
            .AddJwtBearer(authSettings.TokenOptions.Scheme, options =>
            {
                options.Authority = authSettings.TokenOptions.Authority;
                options.Audience = authSettings.TokenOptions.Audience;
                options.RequireHttpsMetadata = authSettings.TokenOptions.RequireHttpsMetadata;
                if (builder.Environment.IsDevelopment())
                {
                    options.BackchannelHttpHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                    };
                }
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                };
            });
        builder.Services.AddAuthorization(options =>
        {
            foreach (var policyOptions in authSettings.Policies)
            {
                options.AddPolicy(policyOptions.Name, policy => policy.RequireAssertion(context => 
                    policyOptions.Claims.TrueForAll(c => context.User.HasClaim(c.Type, c.Value))));
            }
        });
        return builder;
    }
}