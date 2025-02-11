using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Microsoft.OpenApi.Models;
using VehicleService.Data;
using VehicleService.Data.Implementations;
using VehicleService.Data.Interfaces;
using VehicleService.Data.Migrations;
using VehicleService.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddGrpc();
builder.Services.AddGrpcClient<Vehicle.VehicleService.VehicleServiceClient>((provider, options) =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var url = configuration.GetValue<string>("ASPNETCORE_URLS")!.Split(";")[0];
    options.Address = new Uri(url);
});

// Регистрация Dapper
builder.Services.AddSingleton<DatabaseConnectionFactory>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
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
builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb =>
        rb.AddPostgres()
            .WithGlobalConnectionString(builder.Configuration.GetConnectionString("DefaultConnection"))
            .ScanIn(typeof(InitializeMigrations).Assembly).For.Migrations()
    );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseRouting();
app.MapControllers();
app.MapGrpcService<VehicleGrpcService>();

using var scope = app.Services.CreateScope();
var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
runner.MigrateUp();

app.Run();