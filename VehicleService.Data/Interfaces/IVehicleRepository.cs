namespace VehicleService.Data.Interfaces;

public interface IVehicleRepository
{
    Task<IEnumerable<Vehicle>> GetAllAsync();
    Task<Vehicle?> GetByIdAsync(int id);
    Task<int> CreateAsync(Vehicle vehicle);
    Task<bool> UpdateAsync(Vehicle vehicle);
    Task<bool> DeleteAsync(int id);
}