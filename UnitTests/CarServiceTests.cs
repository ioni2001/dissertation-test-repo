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
        public async Task GetAvailableCarsAsync_ReturnsListOfCarResponseDto_WhenCarsAreAvailable()
        {
            // Arrange
            var cars = new List<Car> { new Car { IsAvailable = true }, new Car { IsAvailable = true } };
            _carRepository.GetAvailableCarsAsync().Returns(cars);

            // Act
            var result = await _carService.GetAvailableCarsAsync();

            // Assert
            result.Should().BeOfType<List<CarResponseDto>>();
            result.Should().HaveCount(2);
        }

        [Test]
        public async Task MarkAsUnavailableAsync_ReturnsCarResponseDto_WhenCarIsFoundAndUpdated()
        {
            // Arrange
            int carId = 1;
            var car = new Car { Id = carId, IsAvailable = true };
            var updatedCar = new Car { Id = carId, IsAvailable = false };

            _carRepository.GetByIdAsync(carId).Returns(car);
            _carRepository.UpdateAsync(carId, Arg.Any<Car>()).Returns(updatedCar);

            // Act
            var result = await _carService.MarkAsUnavailableAsync(carId);

            // Assert
            result.Should().BeOfType<CarResponseDto>();
            result.Should().NotBeNull();
            result.IsAvailable.Should().BeFalse();
            _logger.Received().LogInformation("Marked car as unavailable with ID: {CarId}", carId);
        }

        [Test]
        public async Task MarkAsUnavailableAsync_ReturnsNull_WhenCarIsNotFound()
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

            // Act
            var result = await _carService.GetAveragePriceAsync();

            // Assert
            result.Should().Be(15000);
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
    }
}
