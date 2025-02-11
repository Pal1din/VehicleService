using Grpc.Net.Client;
using VehicleService.Data;
using VehicleService.Data.Implementations;
using VehicleService.Data.Interfaces;
using VehicleService.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddGrpc();

// Регистрация Dapper
builder.Services.AddSingleton<DatabaseConnectionFactory>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<Vehicle.VehicleService.VehicleServiceClient>(_ => 
    new Vehicle.VehicleService.VehicleServiceClient(GrpcChannel.ForAddress("https://localhost:7165")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.Run();