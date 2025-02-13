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
    [Authorize(Policy = "read")]
    public async Task<IActionResult> GetVehicles()
    {
        return await SafelyFunction(async () => await grpcClient.ListVehiclesAsync(new Empty()));
    }
    
    [HttpGet("{id}")]
    [Authorize(Policy = "read")]
    public async Task<IActionResult> GetVehicle(int id)
    {
        return await SafelyFunction(async () => await grpcClient.GetVehicleAsync(new VehicleRequest { Id = id }));
    }

    [HttpPost]
    [Authorize(Policy = "write")]
    public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleRequest request)
    {
        return await SafelyFunction(async () => await grpcClient.CreateVehicleAsync(request));
    }

    [HttpDelete]
    [Authorize(Policy = "write")]
    public async Task<IActionResult> DeleteVehicle([FromBody]int id)
    {
        return await SafelyFunction(async () => await grpcClient.DeleteVehicleAsync(new VehicleRequest { Id = id }));
    }

    [HttpPut]
    [Authorize(Policy = "write")]
    public async Task<IActionResult> UpdateVehicle([FromBody] UpdateVehicleRequest request)
    {
        return await SafelyFunction(async () => await grpcClient.UpdateVehicleAsync(request));
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