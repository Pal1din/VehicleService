using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using VehicleService.API;
using VehicleService.API.Extensions;
using VehicleService.Grpc;
using VehicleService.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource("VehicleService")
        .SetSampler(new AlwaysOnSampler()))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddPrometheusExporter());

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateBootstrapLogger();
builder.Host.UseSerilog();

GetSettings(builder, out var swaggerUiSettings, out var authenticationSettings);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder
    .AddVehicleGrpcClientService("https://localhost:7165")
    .AddGrpcServices()
    .AddSwagger()
    .AddMigrator()
    .AddApplicationServices()
    .AddData()
    .AddAuthService(authenticationSettings!);

var app = builder.Build();
app.MapPrometheusScrapingEndpoint();
app.UseSerilogRequestLogging();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint(swaggerUiSettings.Url, swaggerUiSettings.Name);
    c.OAuthClientId(swaggerUiSettings.ClientId);
    c.OAuthClientSecret(swaggerUiSettings.ClientSecret);
    c.OAuthUsePkce();
});

app.UseHttpsRedirection(); 
app.MapControllers();
app.MapGrpcReflectionService();
app.MapGrpcService<VehicleGrpcService>();
app.Migrate();

app.Run();
return;

void GetSettings(WebApplicationBuilder webApplicationBuilder,
    out SwaggerUISettings? swaggerUiSettings1, out AuthenticationSettings authenticationSettings1)
{
    authenticationSettings1 = webApplicationBuilder.Configuration.GetSection("IdentityServer").Get<AuthenticationSettings>();
    swaggerUiSettings1 = webApplicationBuilder.Configuration.GetSection("SwaggerUi").Get<SwaggerUISettings>();
}