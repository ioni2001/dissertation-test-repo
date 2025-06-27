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
        private ICarRepository _carRepository;
        private ILogger<CarService> _logger;
        private CarService _carService;

        [SetUp]
        public void Setup()
        {
            _carRepository = Substitute.For<ICarRepository>();
            _logger = Substitute.For<ILogger<CarService>>();
            _carService = new CarService(_carRepository, _logger);
        }

        [Test]
        public async Task GetAvailableCarsAsync_ReturnsAvailableCars()
        {
            // Arrange
            var cars = new List<Car> { new Car { Id = 1, IsAvailable = true }, new Car { Id = 2, IsAvailable = false } };
            _carRepository.GetAvailableCarsAsync().Returns(Task.FromResult(cars.Where(c => c.IsAvailable)));

            // Act
            var result = await _carService.GetAvailableCarsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(1);
            result.First().Id.Should().Be(1);
        }

        [Test]
        public async Task MarkAsUnavailableAsync_CarExists_MarksCarAsUnavailable()
        {
            // Arrange
            var car = new Car { Id = 1, IsAvailable = true };
            _carRepository.GetByIdAsync(1).Returns(Task.FromResult(car));
            _carRepository.UpdateAsync(1, Arg.Any<Car>()).Returns(Task.FromResult(car));

            // Act
            var result = await _carService.MarkAsUnavailableAsync(1);

            // Assert
            result.Should().NotBeNull();
            car.IsAvailable.Should().BeFalse();
            _carRepository.Received(1).UpdateAsync(1, Arg.Is<Car>(c => !c.IsAvailable));
            _logger.Received(1).LogInformation("Marked car as unavailable with ID: {CarId}", 1);
        }

        [Test]
        public async Task MarkAsUnavailableAsync_CarDoesNotExist_ReturnsNull()
        {
            // Arrange
            _carRepository.GetByIdAsync(1).Returns(Task.FromResult<Car>(null));

            // Act
            var result = await _carService.MarkAsUnavailableAsync(1);

            // Assert
            result.Should().BeNull();
            _carRepository.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<Car>());
            _logger.DidNotReceive().LogInformation(Arg.Any<string>(), Arg.Any<object[]>());
        }

        [Test]
        public async Task GetAveragePriceAsync_CarsExist_ReturnsAveragePrice()
        {
            // Arrange
            var cars = new List<Car> { new Car { Price = 10000 }, new Car { Price = 20000 }, new Car { Price = 30000 } };
            _carRepository.GetAllAsync().Returns(Task.FromResult((IEnumerable<Car>)cars));

            // Act
            var result = await _carService.GetAveragePriceAsync();

            // Assert
            result.Should().Be(20000);
        }

        [Test]
        public async Task GetAveragePriceAsync_NoCarsExist_ReturnsZero()
        {
            // Arrange
            _carRepository.GetAllAsync().Returns(Task.FromResult(Enumerable.Empty<Car>()));

            // Act
            var result = await _carService.GetAveragePriceAsync();

            // Assert
            result.Should().Be(0);
        }
    }
}
