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

        [Test]
        public async Task GetAvailableCarsAsync_ReturnsAvailableCars()
        {
            // Arrange
            var cars = new List<Car> { new Car { IsAvailable = true }, new Car { IsAvailable = false } };
            _carRepository.GetAvailableCarsAsync().Returns(cars);

            // Act
            var result = await _carService.GetAvailableCarsAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Test]
        public async Task MarkAsUnavailableAsync_CarExists_MarksCarAsUnavailable()
        {
            // Arrange
            int carId = 1;
            var car = new Car { Id = carId, IsAvailable = true };
            _carRepository.GetByIdAsync(carId).Returns(car);
            _carRepository.UpdateAsync(carId, Arg.Any<Car>()).Returns(car);

            // Act
            var result = await _carService.MarkAsUnavailableAsync(carId);

            // Assert
            result.Should().NotBeNull();
            _carRepository.Received(1).UpdateAsync(carId, Arg.Is<Car>(c => c.IsAvailable == false));
        }

        [Test]
        public async Task MarkAsUnavailableAsync_CarDoesNotExist_ReturnsNull()
        {
            // Arrange
            int carId = 1;
            _carRepository.GetByIdAsync(carId).Returns((Car)null);

            // Act
            var result = await _carService.MarkAsUnavailableAsync(carId);

            // Assert
            result.Should().BeNull();
            _carRepository.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<Car>());
        }

        [Test]
        public async Task GetAveragePriceAsync_ReturnsAveragePrice()
        {
            // Arrange
            var cars = new List<Car> { new Car { Price = 10 }, new Car { Price = 20 }, new Car { Price = 30 } };
            _carRepository.GetAllAsync().Returns(cars);

            // Act
            var result = await _carService.GetAveragePriceAsync();

            // Assert
            result.Should().Be(20);
        }

        [Test]
        public async Task GetAveragePriceAsync_NoCars_ReturnsZero()
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
