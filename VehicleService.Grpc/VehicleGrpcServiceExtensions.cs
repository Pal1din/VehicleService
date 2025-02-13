namespace VehicleService.Grpc;

public static class VehicleGrpcServiceExtensions
{
    public static WebApplicationBuilder AddGrpcServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddGrpc();
        builder.Services.AddGrpcReflection();
        return builder;
    }
    public static WebApplicationBuilder AddVehicleGrpcClientService(this WebApplicationBuilder builder, string baseUrl)
    {
        builder.Services.AddGrpcClient<Vehicle.VehicleService.VehicleServiceClient>(options =>
        {
            options.Address = new Uri(baseUrl);
        });
        return builder;
    }
}