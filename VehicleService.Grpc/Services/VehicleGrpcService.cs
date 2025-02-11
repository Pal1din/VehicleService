using Grpc.Core;
using Vehicle;
using VehicleService.Data.Interfaces;

namespace VehicleService.Grpc.Services;

public class VehicleGrpcService(IVehicleRepository repository) : Vehicle.VehicleService.VehicleServiceBase
{
    public override async Task<VehicleResponse> GetVehicle(VehicleRequest request, ServerCallContext context)
    {
        var vehicle = await repository.GetByIdAsync(request.Id);
        if (vehicle == null) throw new RpcException(new Status(StatusCode.NotFound, "Vehicle not found"));

        return new VehicleResponse
        {
            Id = vehicle.Id,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Vin = vehicle.Vin,
            LicensePlate = vehicle.LicensePlate
        };
    }

    public override async Task<VehicleListResponse> ListVehicles(Empty request, ServerCallContext context)
    {
        var vehicles = await repository.GetAllAsync();
        var response = new VehicleListResponse();
        response.Vehicles.AddRange(vehicles.Select(v => new VehicleResponse
        {
            Id = v.Id,
            Make = v.Make,
            Model = v.Model,
            Year = v.Year,
            Vin = v.Vin,
            LicensePlate = v.LicensePlate
        }));
        return response;
    }
}