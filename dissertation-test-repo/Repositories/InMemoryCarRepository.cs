using dissertation_test_repo.Models;
using System.Collections.Concurrent;

namespace dissertation_test_repo.Repositories
{
    public class InMemoryCarRepository : ICarRepository
    {
        private readonly ConcurrentDictionary<int, Car> _cars;
        private int _nextId = 1;

        public InMemoryCarRepository()
        {
            _cars = new ConcurrentDictionary<int, Car>();
            SeedData();
        }

        private void SeedData()
        {
            var initialCars = new List<Car>
            {
                new Car { Id = _nextId++, Make = "Toyota", Model = "Camry", Year = 2022, Color = "Silver", Price = 28000, IsAvailable = true },
                new Car { Id = _nextId++, Make = "Honda", Model = "Civic", Year = 2023, Color = "Blue", Price = 25000, IsAvailable = true },
                new Car { Id = _nextId++, Make = "Ford", Model = "Mustang", Year = 2021, Color = "Red", Price = 35000, IsAvailable = false },
                new Car { Id = _nextId++, Make = "BMW", Model = "X3", Year = 2023, Color = "Black", Price = 45000, IsAvailable = true },
                new Car { Id = _nextId++, Make = "Audi", Model = "A4", Year = 2022, Color = "White", Price = 42000, IsAvailable = true }
            };

            foreach (var car in initialCars)
            {
                _cars.TryAdd(car.Id, car);
            }
        }

        public Task<IEnumerable<Car>> GetAllAsync()
        {
            return Task.FromResult(_cars.Values.AsEnumerable());
        }

        public Task<Car?> GetByIdAsync(int id)
        {
            _cars.TryGetValue(id, out var car);
            return Task.FromResult(car);
        }

        public Task<Car> CreateAsync(Car car)
        {
            car.Id = _nextId++;
            car.CreatedAt = DateTime.UtcNow;
            _cars.TryAdd(car.Id, car);
            return Task.FromResult(car);
        }

        public Task<Car?> UpdateAsync(int id, Car updatedCar)
        {
            if (_cars.TryGetValue(id, out var existingCar))
            {
                updatedCar.Id = id;
                updatedCar.CreatedAt = existingCar.CreatedAt;
                updatedCar.UpdatedAt = DateTime.UtcNow;
                _cars.TryUpdate(id, updatedCar, existingCar);
                return Task.FromResult<Car?>(updatedCar);
            }
            return Task.FromResult<Car?>(null);
        }

        public Task<bool> DeleteAsync(int id)
        {
            return Task.FromResult(_cars.TryRemove(id, out _));
        }

        public Task<IEnumerable<Car>> GetByMakeAsync(string make)
        {
            var cars = _cars.Values.Where(c =>
                string.Equals(c.Make, make, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(cars);
        }

        public Task<IEnumerable<Car>> GetAvailableCarsAsync()
        {
            var availableCars = _cars.Values.Where(c => c.IsAvailable);
            return Task.FromResult(availableCars);
        }
    }
}
