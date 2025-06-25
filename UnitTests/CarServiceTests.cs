using dissertation_test_repo.DTOs;
using dissertation_test_repo.Models;
using dissertation_test_repo.Repositories;
using dissertation_test_repo.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using FluentAssertions;

namespace dissertation_test_repo.Tests.Services
{
    [TestFixture]
    public class CarServiceTests
    {
        private CarService _carService;
        private ICarRepository _carRepository;
        private ILogger<CarService> _logger;

        [SetUp]
        public void Setup()
        {
            _carRepository = Substitute.For<ICarRepository>();
            _logger = Substitute.For<ILogger<CarService>>();
            _carService = new CarService(_carRepository, _logger);
        }

        #region GetAllCarsAsync Tests

        [Test]
        public async Task GetAllCarsAsync_ReturnsListOfCarResponseDto_WhenCarsExist()
        {
            // Arrange
            var cars = new List<Car>
            {
                new Car { Id = 1, Make = "Toyota", Model = "Corolla", Year = 2020, Color = "Blue", Price = 20000, IsAvailable = true },
                new Car { Id = 2, Make = "Honda", Model = "Civic", Year = 2021, Color = "Red", Price = 22000, IsAvailable = false }
            };
            _carRepository.GetAllAsync().Returns(cars);

            // Act
            var result = await _carService.GetAllCarsAsync();

            // Assert
            var resultList = result.ToList();
            resultList[0].Id.Should().Be(1);
            resultList[0].Make.Should().Be("Toyota");
            resultList[1].Id.Should().Be(2);
            resultList[1].Make.Should().Be("Honda");
        }

        [Test]
        public async Task GetAllCarsAsync_ReturnsEmptyList_WhenNoCarsExist()
        {
            // Arrange
            _carRepository.GetAllAsync().Returns(new List<Car>());

            // Act
            var result = await _carService.GetAllCarsAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetCarByIdAsync Tests

        [Test]
        public async Task GetCarByIdAsync_ReturnsCarResponseDto_WhenCarExists()
        {
            // Arrange
            var car = new Car { Id = 1, Make = "Toyota", Model = "Corolla", Year = 2020, Color = "Blue", Price = 20000, IsAvailable = true };
            _carRepository.GetByIdAsync(1).Returns(car);

            // Act
            var result = await _carService.GetCarByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Make.Should().Be("Toyota");
            result.Model.Should().Be("Corolla");
        }

        [Test]
        public async Task GetCarByIdAsync_ReturnsNull_WhenCarDoesNotExist()
        {
            // Arrange
            _carRepository.GetByIdAsync(1).Returns((Car)null);

            // Act
            var result = await _carService.GetCarByIdAsync(1);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateCarAsync Tests

        [Test]
        public async Task CreateCarAsync_ReturnsCarResponseDto_WhenCarIsUnique()
        {
            // Arrange
            var carDto = new CarCreateDto { Make = "Toyota", Model = "Corolla", Year = 2020, Color = "Blue", Price = 20000 };
            var existingCars = new List<Car>();
            var createdCar = new Car { Id = 1, Make = "Toyota", Model = "Corolla", Year = 2020, Color = "Blue", Price = 20000, IsAvailable = true };

            _carRepository.GetAllAsync().Returns(existingCars);
            _carRepository.CreateAsync(Arg.Any<Car>()).Returns(createdCar);

            // Act
            var result = await _carService.CreateCarAsync(carDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Make.Should().Be("Toyota");
            await _carRepository.Received(1).CreateAsync(Arg.Any<Car>());
        }

        [Test]
        public async Task CreateCarAsync_ThrowsInvalidOperationException_WhenDuplicateCarExists()
        {
            // Arrange
            var carDto = new CarCreateDto { Make = "Toyota", Model = "Corolla", Year = 2020, Color = "Blue", Price = 20000 };
            var existingCars = new List<Car>
            {
                new Car { Id = 1, Make = "Toyota", Model = "Corolla", Year = 2020, Color = "Blue", Price = 20000, IsAvailable = true }
            };

            _carRepository.GetAllAsync().Returns(existingCars);

            // Act & Assert
            var act = () => _carService.CreateCarAsync(carDto);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("A car with the same make, model, year, and color already exists.");

            await _carRepository.DidNotReceive().CreateAsync(Arg.Any<Car>());
        }

        [Test]
        public async Task CreateCarAsync_ThrowsInvalidOperationException_WhenDuplicateCarExistsCaseInsensitive()
        {
            // Arrange
            var carDto = new CarCreateDto { Make = "TOYOTA", Model = "COROLLA", Year = 2020, Color = "BLUE", Price = 20000 };
            var existingCars = new List<Car>
            {
                new Car { Id = 1, Make = "toyota", Model = "corolla", Year = 2020, Color = "blue", Price = 20000, IsAvailable = true }
            };

            _carRepository.GetAllAsync().Returns(existingCars);

            // Act & Assert
            var act = () => _carService.CreateCarAsync(carDto);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("A car with the same make, model, year, and color already exists.");
        }

        #endregion

        #region UpdateCarAsync Tests

        [Test]
        public async Task UpdateCarAsync_ReturnsUpdatedCarResponseDto_WhenCarExistsAndUpdateSucceeds()
        {
            // Arrange
            var existingCar = new Car { Id = 1, Make = "Toyota", Model = "Corolla", Year = 2020, Color = "Blue", Price = 20000, IsAvailable = true };
            var updateDto = new CarUpdateDto { Make = "Honda", Model = "Civic", Year = 2021, Color = "Red", Price = 25000, IsAvailable = false };
            var updatedCar = new Car { Id = 1, Make = "Honda", Model = "Civic", Year = 2021, Color = "Red", Price = 25000, IsAvailable = false };

            _carRepository.GetByIdAsync(1).Returns(existingCar);
            _carRepository.UpdateAsync(1, Arg.Any<Car>()).Returns(updatedCar);

            // Act
            var result = await _carService.UpdateCarAsync(1, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Make.Should().Be("Honda");
            result.Model.Should().Be("Civic");
            result.Year.Should().Be(2021);
            result.Color.Should().Be("Red");
            result.Price.Should().Be(25000);
            result.IsAvailable.Should().BeFalse();
        }

        [Test]
        public async Task UpdateCarAsync_ReturnsNull_WhenCarDoesNotExist()
        {
            // Arrange
            var updateDto = new CarUpdateDto { Make = "Honda" };
            _carRepository.GetByIdAsync(1).Returns((Car)null);

            // Act
            var result = await _carService.UpdateCarAsync(1, updateDto);

            // Assert
            result.Should().BeNull();
            await _carRepository.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<Car>());
        }

        [Test]
        public async Task UpdateCarAsync_ReturnsNull_WhenUpdateFails()
        {
            // Arrange
            var existingCar = new Car { Id = 1, Make = "Toyota", Model = "Corolla", Year = 2020, Color = "Blue", Price = 20000, IsAvailable = true };
            var updateDto = new CarUpdateDto { Make = "Honda" };

            _carRepository.GetByIdAsync(1).Returns(existingCar);
            _carRepository.UpdateAsync(1, Arg.Any<Car>()).Returns((Car)null);

            // Act
            var result = await _carService.UpdateCarAsync(1, updateDto);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task UpdateCarAsync_UpdatesOnlyProvidedFields_WhenPartialUpdateProvided()
        {
            // Arrange
            var existingCar = new Car { Id = 1, Make = "Toyota", Model = "Corolla", Year = 2020, Color = "Blue", Price = 20000, IsAvailable = true };
            var updateDto = new CarUpdateDto { Make = "Honda", Price = 25000 }; // Only updating make and price
            var updatedCar = new Car { Id = 1, Make = "Honda", Model = "Corolla", Year = 2020, Color = "Blue", Price = 25000, IsAvailable = true };

            _carRepository.GetByIdAsync(1).Returns(existingCar);
            _carRepository.UpdateAsync(1, Arg.Any<Car>()).Returns(updatedCar);

            // Act
            var result = await _carService.UpdateCarAsync(1, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Make.Should().Be("Honda"); // Updated
            result.Model.Should().Be("Corolla"); // Unchanged
            result.Year.Should().Be(2020); // Unchanged
            result.Color.Should().Be("Blue"); // Unchanged
            result.Price.Should().Be(25000); // Updated
            result.IsAvailable.Should().BeTrue(); // Unchanged
        }

        [Test]
        public async Task UpdateCarAsync_DoesNotUpdateFields_WhenFieldsAreNullOrEmpty()
        {
            // Arrange
            var existingCar = new Car { Id = 1, Make = "Toyota", Model = "Corolla", Year = 2020, Color = "Blue", Price = 20000, IsAvailable = true };
            var updateDto = new CarUpdateDto { Make = "", Model = null, Year = null, Color = "", Price = null, IsAvailable = null };
            var updatedCar = new Car { Id = 1, Make = "Toyota", Model = "Corolla", Year = 2020, Color = "Blue", Price = 20000, IsAvailable = true };

            _carRepository.GetByIdAsync(1).Returns(existingCar);
            _carRepository.UpdateAsync(1, Arg.Any<Car>()).Returns(updatedCar);

            // Act
            var result = await _carService.UpdateCarAsync(1, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Make.Should().Be("Toyota"); // Unchanged (empty string)
            result.Model.Should().Be("Corolla"); // Unchanged (null)
            result.Year.Should().Be(2020); // Unchanged (null)
            result.Color.Should().Be("Blue"); // Unchanged (empty string)
            result.Price.Should().Be(20000); // Unchanged (null)
            result.IsAvailable.Should().BeTrue(); // Unchanged (null)
        }

        #endregion

        #region DeleteCarAsync Tests

        [Test]
        public async Task DeleteCarAsync_ReturnsTrue_WhenCarExistsAndIsAvailableAndDeleteSucceeds()
        {
            // Arrange
            var car = new Car { Id = 1, Make = "Toyota", Model = "Corolla", Year = 2020, Color = "Blue", Price = 20000, IsAvailable = true };
            _carRepository.GetByIdAsync(1).Returns(car);
            _carRepository.DeleteAsync(1).Returns(true);

            // Act
            var result = await _carService.DeleteCarAsync(1);

            // Assert
            result.Should().BeTrue();
            await _carRepository.Received(1).DeleteAsync(1);
        }

        [Test]
        public async Task DeleteCarAsync_ReturnsFalse_WhenCarDoesNotExist()
        {
            // Arrange
            _carRepository.GetByIdAsync(1).Returns((Car)null);

            // Act
            var result = await _carService.DeleteCarAsync(1);

            // Assert
            result.Should().BeFalse();
            await _carRepository.DidNotReceive().DeleteAsync(Arg.Any<int>());
        }

        [Test]
        public async Task DeleteCarAsync_ThrowsInvalidOperationException_WhenCarIsNotAvailable()
        {
            // Arrange
            var car = new Car { Id = 1, Make = "Toyota", Model = "Corolla", Year = 2020, Color = "Blue", Price = 20000, IsAvailable = false };
            _carRepository.GetByIdAsync(1).Returns(car);

            // Act & Assert
            var act = () => _carService.DeleteCarAsync(1);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Cannot delete a car that is not available.");

            await _carRepository.DidNotReceive().DeleteAsync(Arg.Any<int>());
        }

        [Test]
        public async Task DeleteCarAsync_ReturnsFalse_WhenDeleteFails()
        {
            // Arrange
            var car = new Car { Id = 1, Make = "Toyota", Model = "Corolla", Year = 2020, Color = "Blue", Price = 20000, IsAvailable = true };
            _carRepository.GetByIdAsync(1).Returns(car);
            _carRepository.DeleteAsync(1).Returns(false);

            // Act
            var result = await _carService.DeleteCarAsync(1);

            // Assert
            result.Should().BeFalse();
            await _carRepository.Received(1).DeleteAsync(1);
        }

        #endregion

        #region GetCarsByMakeAsync Tests

        [Test]
        public async Task GetCarsByMakeAsync_ReturnsListOfCarResponseDto_WhenCarsWithMakeExist()
        {
            // Arrange
            var make = "Toyota";
            var cars = new List<Car>
            {
                new Car { Id = 1, Make = "Toyota", Model = "Corolla", Year = 2020, Color = "Blue", Price = 20000, IsAvailable = true },
                new Car { Id = 2, Make = "Toyota", Model = "Camry", Year = 2021, Color = "Red", Price = 25000, IsAvailable = false }
            };
            _carRepository.GetByMakeAsync(make).Returns(cars);

            // Act
            var result = await _carService.GetCarsByMakeAsync(make);

            // Assert
            var resultList = result.ToList();
            resultList.Should().AllSatisfy(car => car.Make.Should().Be("Toyota"));
            resultList[0].Model.Should().Be("Corolla");
            resultList[1].Model.Should().Be("Camry");
        }

        [Test]
        public async Task GetCarsByMakeAsync_ReturnsEmptyList_WhenNoCarsWithMakeExist()
        {
            // Arrange
            var make = "BMW";
            _carRepository.GetByMakeAsync(make).Returns(new List<Car>());

            // Act
            var result = await _carService.GetCarsByMakeAsync(make);

            // Assert
            result.Should().BeEmpty();
        }

        #endregion
    }
}