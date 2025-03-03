using VehicleService.API.Extensions;
using VehicleService.Grpc;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureSerilog()
.AddTelemetryMetrics()
.AddDefault()
.AddAuthService(builder.Configuration.GetSection("IdentityServer"))
.AddApplicationCors()
.AddGrpcServices()
.AddSwagger()
.AddMigrator()
.AddApplicationServices()
.AddDbContext()
.AddRepositories();

var app = builder.Build();
app.ConfigureApplication(app.Configuration.GetSection("SwaggerUi"));
await app.MigrateAndRun();