using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vehicle;
using VehicleService.API.Extensions;
using VehicleService.Grpc;

namespace VehicleService.API.Controllers;

[Route("api/vehicles")]
[ApiController]
public class VehicleController(Vehicle.VehicleService.VehicleServiceClient grpcClient) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "CanRead")]
    public async Task<IActionResult> GetVehicles()
    {
        return await SafelyFunction(async () => 
            await grpcClient.ListVehiclesAsync(new Empty(), HttpContext.AddTokenToMetadata()));
    }
    
    [HttpGet("{id}")]
    [Authorize(Policy = "CanRead")]
    public async Task<IActionResult> GetVehicle(int id)
    {
        return await SafelyFunction(async () => 
            await grpcClient.GetVehicleAsync(new VehicleRequest { Id = id }, HttpContext.AddTokenToMetadata()));
    }

    [HttpPost]
    [Authorize(Policy = "CanWrite")]
    public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleRequest request)
    {
        return await SafelyFunction(async () => await grpcClient.CreateVehicleAsync(request, HttpContext.AddTokenToMetadata()));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CanWrite")]
    public async Task<IActionResult> DeleteVehicle(int id)
    {
        return await SafelyFunction(async () => await grpcClient.DeleteVehicleAsync(new VehicleRequest { Id = id }, HttpContext.AddTokenToMetadata()));
    }

    [HttpPut]
    [Authorize(Policy = "CanWrite")]
    public async Task<IActionResult> UpdateVehicle([FromBody] UpdateVehicleRequest request)
    {
        return await SafelyFunction(async () => await grpcClient.UpdateVehicleAsync(request, HttpContext.AddTokenToMetadata()));
    }

    private async Task<IActionResult> SafelyFunction<TResult>(Func<Task<TResult>> func)
    {
        try
        {
            var response = await func();
            if (response?.GetType() == typeof(Empty))
            {
                return Ok("Success");
            }
            return Ok(response);
        }
        catch (RpcException e)
        {
            return e.StatusCode switch
            {
                global::Grpc.Core.StatusCode.NotFound => NotFound(e.Message),
                global::Grpc.Core.StatusCode.PermissionDenied => Forbid(),
                _ => StatusCode(500, e.Message),
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}