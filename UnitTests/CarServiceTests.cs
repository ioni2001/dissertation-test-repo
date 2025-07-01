using dissertation_test_repo.DTOs;
using dissertation_test_repo.Models;
using dissertation_test_repo.Repositories;
using dissertation_test_repo.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        [Test]
        public async Task GetAvailableCarsAsync_ReturnsListOfCarResponseDtos()
        {
            // Arrange
            var cars = new List<Car> { new Car(), new Car() };
            _carRepository.GetAvailableCarsAsync().Returns(cars);

            // Act
            var result = await _carService.GetAvailableCarsAsync();

            // Assert
            result.Should().BeOfType<List<CarResponseDto>>().And.HaveCount(cars.Count);
        }

        [Test]
        public async Task MarkAsUnavailableAsync_ReturnsCarResponseDto_WhenCarIsUpdated()
        {
            // Arrange
            int carId = 1;
            var car = new Car { Id = carId, IsAvailable = true };
            _carRepository.GetByIdAsync(carId).Returns(car);
            _carRepository.UpdateAsync(carId, Arg.Any<Car>()).Returns(car);

            // Act
            var result = await _carService.MarkAsUnavailableAsync(carId);

            // Assert
            result.Should().BeOfType<CarResponseDto>();
            car.IsAvailable.Should().BeFalse();
        }

        [Test]
        public async Task MarkAsUnavailableAsync_ReturnsNull_WhenCarDoesNotExist()
        {
            // Arrange
            int carId = 1;
            _carRepository.GetByIdAsync(carId).Returns((Car)null);

            // Act
            var result = await _carService.MarkAsUnavailableAsync(carId);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task GetAveragePriceAsync_ReturnsAveragePrice_WhenCarsExist()
        {
            // Arrange
            var cars = new List<Car> { new Car { Price = 10000 }, new Car { Price = 20000 } };
            _carRepository.GetAllAsync().Returns(cars);
            double expectedAveragePrice = 15000;

            // Act
            var result = await _carService.GetAveragePriceAsync();

            // Assert
            result.Should().Be(expectedAveragePrice);
        }

        [Test]
        public async Task GetAveragePriceAsync_ReturnsZero_WhenNoCarsExist()
        {
            // Arrange
            _carRepository.GetAllAsync().Returns(new List<Car>());

            // Act
            var result = await _carService.GetAveragePriceAsync();

            // Assert
            result.Should().Be(0);
        }

        [Test]
        public async Task DeleteCarAsync_ReturnsFalse_WhenCarNotFound()
        {
            // Arrange
            int carId = 1;
            _carRepository.GetByIdAsync(carId).Returns((Car)null);

            // Act
            var result = await _carService.DeleteCarAsync(carId);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task DeleteCarAsync_ThrowsInvalidOperationException_WhenCarIsNotAvailable()
        {
            // Arrange
            int carId = 1;
            var car = new Car { Id = carId, IsAvailable = false };
            _carRepository.GetByIdAsync(carId).Returns(car);

            // Act
            Func<Task<bool>> act = async () => await _carService.DeleteCarAsync(carId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Cannot delete a car that is not available.");
        }

        [Test]
        public async Task DeleteCarAsync_ReturnsTrue_WhenCarIsDeleted()
        {
            // Arrange
            int carId = 1;
            var car = new Car { Id = carId, IsAvailable = true };
            _carRepository.GetByIdAsync(carId).Returns(car);
            _carRepository.DeleteAsync(carId).Returns(true);

            // Act
            var result = await _carService.DeleteCarAsync(carId);

            // Assert
            result.Should().BeTrue();
        }
    }
}
