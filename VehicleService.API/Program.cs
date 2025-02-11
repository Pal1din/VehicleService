using VehicleService.API.Extensions;
using VehicleService.Grpc;
using VehicleService.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder
    .AddVehicleGrpcClientService("https://localhost:7165")
    .AddGrpcServices()
    .AddSwagger()
    .AddMigrator()
    .AddApplicationServices()
    .AddData()
    .AddAuthService();

var app = builder.Build();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection(); 
app.MapControllers();
app.MapGrpcService<VehicleGrpcService>();
app.Migrate();

app.Run();