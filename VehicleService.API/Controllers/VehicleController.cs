using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vehicle;

namespace VehicleService.API.Controllers;

[Route("api/vehicles")]
[ApiController]
[Authorize]
public class VehicleController(Vehicle.VehicleService.VehicleServiceClient grpcClient) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetVehicles()
    {
        return await SafelyFunction(async () => await grpcClient.ListVehiclesAsync(new Empty()));
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetVehicle(int id)
    {
        return await SafelyFunction(async () => await grpcClient.GetVehicleAsync(new VehicleRequest { Id = id }));
    }

    [HttpPost]
    public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleRequest request)
    {
        return await SafelyFunction(async () => await grpcClient.CreateVehicleAsync(request));
    }

    private async Task<IActionResult> SafelyFunction<TResult>(Func<Task<TResult>> func)
    {
        try
        {
            var response = await func();
            return Ok(response);
        }
        catch (RpcException e)
        {
            return e.StatusCode switch
            {
                global::Grpc.Core.StatusCode.NotFound => NotFound(e.Message),
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