using System.Security.Claims;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Vehicle;
using VehicleService.Data.Entities;
using VehicleService.Data.Interfaces;

namespace VehicleService.Grpc.Services;

public class VehicleGrpcService(IVehicleRepository repository)
    : Vehicle.VehicleService.VehicleServiceBase
{
    [Authorize(Policy = "CanRead")]
    public override async Task<VehicleResponse> GetVehicle(VehicleRequest request, ServerCallContext context)
    {
        return await Act(context, Get, request.Id);

        async Task<VehicleResponse> Get()
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
    }

    [Authorize(Policy = "CanWrite")]
    public override async Task<VehicleResponse> CreateVehicle(CreateVehicleRequest request, ServerCallContext context)
    {
        var userId = GetUserId(GetUserFromContext(context));
        var vehicle = new VehicleEntity
        {
            LicensePlate = request.LicensePlate,
            Make = request.Make,
            Model = request.Model,
            Year = request.Year,
            Vin = request.Vin,
            OwnerId = userId,
            OrganizationId = request.CreateOnOrganization ? GetOrganizationId(GetUserFromContext(context)) : null
        };
        var id = await repository.CreateAsync(vehicle);
        return new VehicleResponse
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
        var vehicles = IsAdmin(GetUserFromContext(context))
            ? await repository.GetAllAsync()
            : await repository.GetAllAsync(GetUserId(GetUserFromContext(context)), GetOrganizationId(GetUserFromContext(context)));
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
        return await Act(context, Delete, request.Id);
        async Task<Empty> Delete()
        {
            var success = await repository.DeleteAsync(request.Id);
            if (!success) throw new RpcException(new Status(StatusCode.NotFound, "Vehicle not found"));
            return new Empty();
        }
    }

    [Authorize(Policy = "CanWrite")]
    public override async Task<VehicleResponse> UpdateVehicle(UpdateVehicleRequest request, ServerCallContext context)
    {
        return await Act(context, Update, request.Id);

        async Task<VehicleResponse> Update()
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

    private async Task<TResult> Act<TResult>(ServerCallContext context, Func<Task<TResult>> func, int vehicleId)
    {
        if (IsAdmin(GetUserFromContext(context)))
        {
            return await func.Invoke();
        }

        var userId = GetUserId(GetUserFromContext(context));
        var organizationId = GetOrganizationId(GetUserFromContext(context));
        var vehicle = await repository.GetByIdAsync(vehicleId);
        if (vehicle?.OwnerId != userId && vehicle?.OrganizationId != organizationId)
            throw new RpcException(new Status(StatusCode.PermissionDenied,
                $"You are not authorized to {func.Method.Name.ToLower()} this vehicle"));
        return await func.Invoke();
    }

    private ClaimsPrincipal GetUserFromContext(ServerCallContext context) => context.GetHttpContext().User;
    private int GetUserId(ClaimsPrincipal user) => int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
    private int? GetOrganizationId(ClaimsPrincipal user)
    {
        var success = int.TryParse(user.FindFirst("organization_id")?.Value, out int organizationId);
        return success ? organizationId : null;
    }

    private bool IsAdmin(ClaimsPrincipal user) => user.IsInRole("Admin");
}