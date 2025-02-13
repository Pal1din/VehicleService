using VehicleService.API;
using VehicleService.API.Extensions;
using VehicleService.Grpc;
using VehicleService.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);
var authenticationSettings = builder.Configuration.GetSection("IdentityServer").Get<AuthenticationSettings>();
var swaggerUiSettings = builder.Configuration.GetSection("SwaggerUi").Get<SwaggerUISettings>();
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