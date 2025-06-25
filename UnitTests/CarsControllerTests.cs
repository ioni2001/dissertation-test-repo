using dissertation_test_repo.Controllers;
using dissertation_test_repo.DTOs;
using dissertation_test_repo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace dissertation_test_repo.Tests.Controllers
{
    [TestFixture]
    public class CarsControllerTests
    {
        private CarsController _controller;
        private ICarService _carService;
        private ILogger<CarsController> _logger;

        [SetUp]
        public void Setup()
        {
            _carService = Substitute.For<ICarService>();
            _logger = Substitute.For<ILogger<CarsController>>();
            _controller = new CarsController(_carService, _logger);
        }

        [Test]
        public async Task GetAvailableCars_ReturnsOkResultWithCars()
        {
            // Arrange
            var cars = new List<CarResponseDto> { new CarResponseDto(), new CarResponseDto() };
            _carService.GetAvailableCarsAsync().Returns(cars);

            // Act
            var result = await _controller.GetAvailableCars();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(cars);
        }

        [Test]
        public async Task GetAvailableCars_ExceptionThrown_ReturnsStatusCode500()
        {
            // Arrange
            _carService.GetAvailableCarsAsync().Returns(Task.FromException<IEnumerable<CarResponseDto>>(new Exception("Test Exception")));

            // Act
            var result = await _controller.GetAvailableCars();

            // Assert
            result.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Test]
        public async Task MarkAsUnavailable_CarExists_ReturnsOkResultWithCar()
        {
            // Arrange
            int carId = 1;
            var car = new CarResponseDto { Id = carId };
            _carService.MarkAsUnavailableAsync(carId).Returns(car);

            // Act
            var result = await _controller.MarkAsUnavailable(carId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(car);
        }

        [Test]
        public async Task MarkAsUnavailable_CarDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            int carId = 1;
            _carService.MarkAsUnavailableAsync(carId).Returns((CarResponseDto)null);

            // Act
            var result = await _controller.MarkAsUnavailable(carId);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Test]
        public async Task MarkAsUnavailable_ExceptionThrown_ReturnsStatusCode500()
        {
            // Arrange
            int carId = 1;
            _carService.MarkAsUnavailableAsync(carId).Returns(Task.FromException<CarResponseDto>(new Exception("Test Exception")));

            // Act
            var result = await _controller.MarkAsUnavailable(carId);

            // Assert
            result.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Test]
        public async Task GetAveragePrice_ReturnsOkResultWithAveragePrice()
        {
            // Arrange
            decimal averagePrice = 10000;
            _carService.GetAveragePriceAsync().Returns(averagePrice);

            // Act
            var result = await _controller.GetAveragePrice();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be(averagePrice);
        }

        [Test]
        public async Task GetAveragePrice_ExceptionThrown_ReturnsStatusCode500()
        {
            // Arrange
            _carService.GetAveragePriceAsync().Returns(Task.FromException<decimal>(new Exception("Test Exception")));

            // Act
            var result = await _controller.GetAveragePrice();

            // Assert
            result.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }
    }
}
