namespace VehicleService.Grpc;

public static class VehicleGrpcServiceExtensions
{
    public static WebApplicationBuilder AddGrpcServices(this WebApplicationBuilder builder)
    {
        var addresses = builder.Configuration.GetValue<string>("ASPNETCORE_URLS")!.Split(";");
        var address = addresses.FirstOrDefault(x => x.StartsWith("https://"))
            ?? addresses.FirstOrDefault(x => x.StartsWith("http://")) ?? addresses[0];
        builder.Services.AddGrpc();
        builder.Services.AddGrpcReflection();
        builder.Services.AddGrpcClient<Vehicle.VehicleService.VehicleServiceClient>(options =>
        {
            options.Address = new Uri(address);
        });
        return builder;
    }
}