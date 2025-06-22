using dissertation_test_repo.DTOs;
using dissertation_test_repo.Models;
using dissertation_test_repo.Repositories;

namespace dissertation_test_repo.Services
{
    public class CarService : ICarService
    {
        private readonly ICarRepository _carRepository;
        private readonly ILogger<CarService> _logger;

        public CarService(ICarRepository carRepository, ILogger<CarService> logger)
        {
            _carRepository = carRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<CarResponseDto>> GetAllCarsAsync()
        {
            var cars = await _carRepository.GetAllAsync();
            return cars.Select(MapToResponseDto);
        }

        public async Task<CarResponseDto?> GetCarByIdAsync(int id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            return car != null ? MapToResponseDto(car) : null;
        }

        public async Task<CarResponseDto> CreateCarAsync(CarCreateDto carDto)
        {
            // Business logic: Check for duplicate cars
            var existingCars = await _carRepository.GetAllAsync();
            var isDuplicate = existingCars.Any(c =>
                string.Equals(c.Make, carDto.Make, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(c.Model, carDto.Model, StringComparison.OrdinalIgnoreCase) &&
                c.Year == carDto.Year &&
                string.Equals(c.Color, carDto.Color, StringComparison.OrdinalIgnoreCase));

            if (isDuplicate)
            {
                _logger.LogWarning("Attempting to create duplicate car: {Make} {Model} {Year} {Color}",
                    carDto.Make, carDto.Model, carDto.Year, carDto.Color);
                throw new InvalidOperationException("A car with the same make, model, year, and color already exists.");
            }

            var car = MapToEntity(carDto);
            var createdCar = await _carRepository.CreateAsync(car);

            _logger.LogInformation("Created new car with ID: {CarId}", createdCar.Id);
            return MapToResponseDto(createdCar);
        }

        public async Task<CarResponseDto?> UpdateCarAsync(int id, CarUpdateDto carDto)
        {
            var existingCar = await _carRepository.GetByIdAsync(id);
            if (existingCar == null)
            {
                return null;
            }

            // Apply partial updates
            if (!string.IsNullOrEmpty(carDto.Make))
                existingCar.Make = carDto.Make;
            if (!string.IsNullOrEmpty(carDto.Model))
                existingCar.Model = carDto.Model;
            if (carDto.Year.HasValue)
                existingCar.Year = carDto.Year.Value;
            if (!string.IsNullOrEmpty(carDto.Color))
                existingCar.Color = carDto.Color;
            if (carDto.Price.HasValue)
                existingCar.Price = carDto.Price.Value;
            if (carDto.IsAvailable.HasValue)
                existingCar.IsAvailable = carDto.IsAvailable.Value;

            var updatedCar = await _carRepository.UpdateAsync(id, existingCar);

            if (updatedCar != null)
            {
                _logger.LogInformation("Updated car with ID: {CarId}", id);
            }

            return updatedCar != null ? MapToResponseDto(updatedCar) : null;
        }

        public async Task<bool> DeleteCarAsync(int id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null)
            {
                return false;
            }

            // Business logic: Don't allow deletion of unavailable cars (might be in transaction)
            if (!car.IsAvailable)
            {
                _logger.LogWarning("Attempting to delete unavailable car with ID: {CarId}", id);
                throw new InvalidOperationException("Cannot delete a car that is not available.");
            }

            var deleted = await _carRepository.DeleteAsync(id);
            if (deleted)
            {
                _logger.LogInformation("Deleted car with ID: {CarId}", id);
            }

            return deleted;
        }

        public async Task<IEnumerable<CarResponseDto>> GetCarsByMakeAsync(string make)
        {
            var cars = await _carRepository.GetByMakeAsync(make);
            return cars.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<CarResponseDto>> GetAvailableCarsAsync()
        {
            var cars = await _carRepository.GetAvailableCarsAsync();
            return cars.Select(MapToResponseDto);
        }

        public async Task<CarResponseDto?> MarkAsUnavailableAsync(int id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null)
            {
                return null;
            }

            car.IsAvailable = false;
            var updatedCar = await _carRepository.UpdateAsync(id, car);

            if (updatedCar != null)
            {
                _logger.LogInformation("Marked car as unavailable with ID: {CarId}", id);
            }

            return updatedCar != null ? MapToResponseDto(updatedCar) : null;
        }

        public async Task<decimal> GetAveragePriceAsync()
        {
            var cars = await _carRepository.GetAllAsync();
            return cars.Any() ? cars.Average(c => c.Price) : 0;
        }

        private static CarResponseDto MapToResponseDto(Car car)
        {
            return new CarResponseDto
            {
                Id = car.Id,
                Make = car.Make,
                Model = car.Model,
                Year = car.Year,
                Color = car.Color,
                Price = car.Price,
                IsAvailable = car.IsAvailable,
                CreatedAt = car.CreatedAt,
                UpdatedAt = car.UpdatedAt
            };
        }

        private static Car MapToEntity(CarCreateDto carDto)
        {
            return new Car
            {
                Make = carDto.Make,
                Model = carDto.Model,
                Year = carDto.Year,
                Color = carDto.Color,
                Price = carDto.Price,
                IsAvailable = carDto.IsAvailable
            };
        }
    }
}
