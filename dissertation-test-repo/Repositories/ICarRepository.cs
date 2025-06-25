using dissertation_test_repo.Models;

namespace dissertation_test_repo.Repositories
{
    public interface ICarRepository
    {
        Task<IEnumerable<Car>> GetAllAsync();
        Task<Car?> GetByIdAsync(int id);
        Task<Car> CreateAsync(Car car);
        Task<Car?> UpdateAsync(int id, Car car);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Car>> GetByMakeAsync(string make);
    }
}
