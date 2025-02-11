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
    .AddData();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseRouting();
app.MapControllers();
app.MapGrpcService<VehicleGrpcService>();
app.Migrate();

app.Run();