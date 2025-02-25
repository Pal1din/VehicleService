using VehicleService.Data.Entities;

namespace VehicleService.Data.Interfaces;

public interface IVehicleRepository
{
    Task<IEnumerable<VehicleEntity>> GetAllAsync(int? userId = null);
    Task<VehicleEntity?> GetByIdAsync(int id);
    Task<int> CreateAsync(VehicleEntity vehicleEntity);
    Task<bool> UpdateAsync(VehicleEntity vehicleEntity);
    Task<bool> DeleteAsync(int id);
}