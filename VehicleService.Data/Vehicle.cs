using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleService.Data;

public class Vehicle
{
    public int Id { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string Vin { get; set; }
    public string LicensePlate { get; set; }
}