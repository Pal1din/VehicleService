namespace VehicleService.Data.Entities;

public abstract class BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}