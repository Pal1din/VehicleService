using System.Runtime.CompilerServices;
using Grpc.Net.Client;
using VehicleService.Data;
using VehicleService.Data.Implementations;
using VehicleService.Data.Interfaces;
using VehicleService.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddGrpc();
builder.Services.AddGrpcClient<Vehicle.VehicleService.VehicleServiceClient>((provider, options) =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var url = configuration.GetValue<string>("ASPNETCORE_URLS")!.Split(";")[0];
    options.Address = new Uri("url");
});

// Регистрация Dapper
builder.Services.AddSingleton<DatabaseConnectionFactory>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();

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