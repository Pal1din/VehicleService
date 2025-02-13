using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Vehicle;
using VehicleService.Data.Entities;
using VehicleService.Data.Interfaces;

namespace VehicleService.Grpc.Services;

public class VehicleGrpcService(IVehicleRepository repository) : Vehicle.VehicleService.VehicleServiceBase
{
    [Authorize(Policy = "CanRead")]
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

    [Authorize(Policy = "CanWrite")]
    public override async Task<VehicleResponse> CreateVehicle(CreateVehicleRequest request, ServerCallContext context)
    {
        var vehicle = new Data.Entities.VehicleEntity
        {
            LicensePlate = request.LicensePlate,
            Make = request.Make,
            Model = request.Model,
            Year = request.Year,
            Vin = request.Vin
        };
        var id = await repository.CreateAsync(vehicle);
        return new VehicleResponse()
        {
            Id = id,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Vin = vehicle.Vin,
            LicensePlate = vehicle.LicensePlate
        };
    }

    [Authorize(Policy = "CanRead")]
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

    [Authorize(Policy = "CanWrite")]
    public override async Task<Empty> DeleteVehicle(VehicleRequest request, ServerCallContext context)
    {
        var success = await repository.DeleteAsync(request.Id);
        if (!success) throw new RpcException(new Status(StatusCode.NotFound, "Vehicle not found"));
        return new Empty();
    }

    [Authorize(Policy = "CanWrite")]
    public override async Task<VehicleResponse> UpdateVehicle(UpdateVehicleRequest request, ServerCallContext context)
    {
        var vehicle = new VehicleEntity
        {
            Id = request.Id,
            LicensePlate = request.LicensePlate,
            Make = request.Make,
            Model = request.Model,
            Year = request.Year,
            Vin = request.Vin,
        };
        var success = await repository.UpdateAsync(vehicle);
        if (!success) throw new RpcException(new Status(StatusCode.NotFound, "Vehicle not found"));
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
}