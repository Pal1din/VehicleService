using Microsoft.AspNetCore.Mvc;
using Vehicle;

namespace VehicleService.API.Controllers;

[Route("api/vehicles")]
[ApiController]
public class VehicleController(Vehicle.VehicleService.VehicleServiceClient grpcClient) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetVehicle(int id)
    {
        var response = await grpcClient.GetVehicleAsync(new VehicleRequest { Id = id });
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleRequest request)
    {
        var response = await grpcClient.CreateVehicleAsync(request);
        return Ok(response);
    }
}