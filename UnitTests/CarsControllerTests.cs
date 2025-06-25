using dissertation_test_repo.Controllers;
using dissertation_test_repo.DTOs;
using dissertation_test_repo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using FluentAssertions;
using System;
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
        public async Task GetAvailableCars_ReturnsOkResult_WhenCarsExist()
        {
            // Arrange
            var cars = new List<CarResponseDto> { new CarResponseDto { Id = 1, Make = "Toyota" } };
            _carService.GetAvailableCarsAsync().Returns(cars);

            // Act
            var result = await _controller.GetAvailableCars();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(cars);
        }

        [Test]
        public async Task GetAvailableCars_ReturnsStatusCode500_WhenExceptionIsThrown()
        {
            // Arrange
            _carService.GetAvailableCarsAsync().Returns(Task.FromException<IEnumerable<CarResponseDto>>(new Exception("Test Exception")));

            // Act
            var result = await _controller.GetAvailableCars();

            // Assert
            result.Result.Should().BeOfType<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult.Should().NotBeNull();
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("An error occurred while processing your request");
        }

        [Test]
        public async Task MarkAsUnavailable_ReturnsOkResult_WhenCarIsFound()
        {
            // Arrange
            int carId = 1;
            var car = new CarResponseDto { Id = carId, Make = "Toyota", IsAvailable = false };
            _carService.MarkAsUnavailableAsync(carId).Returns(car);

            // Act
            var result = await _controller.MarkAsUnavailable(carId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(car);
        }

        [Test]
        public async Task MarkAsUnavailable_ReturnsNotFoundResult_WhenCarIsNotFound()
        {
            // Arrange
            int carId = 1;
            _carService.MarkAsUnavailableAsync(carId).Returns((CarResponseDto)null);

            // Act
            var result = await _controller.MarkAsUnavailable(carId);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
            notFoundResult.Value.Should().Be($"Car with ID {carId} not found");
        }

        [Test]
        public async Task MarkAsUnavailable_ReturnsStatusCode500_WhenExceptionIsThrown()
        {
            // Arrange
            int carId = 1;
            _carService.MarkAsUnavailableAsync(carId).Returns(Task.FromException<CarResponseDto>(new Exception("Test Exception")));

            // Act
            var result = await _controller.MarkAsUnavailable(carId);

            // Assert
            result.Result.Should().BeOfType<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult.Should().NotBeNull();
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("An error occurred while processing your request");
        }

        [Test]
        public async Task GetAveragePrice_ReturnsOkResult_WithAveragePrice()
        {
            // Arrange
            decimal averagePrice = 15000.50m;
            _carService.GetAveragePriceAsync().Returns(averagePrice);

            // Act
            var result = await _controller.GetAveragePrice();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().Be(averagePrice);
        }

        [Test]
        public async Task GetAveragePrice_ReturnsStatusCode500_WhenExceptionIsThrown()
        {
            // Arrange
            _carService.GetAveragePriceAsync().Returns(Task.FromException<decimal>(new Exception("Test Exception")));

            // Act
            var result = await _controller.GetAveragePrice();

            // Assert
            result.Result.Should().BeOfType<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult.Should().NotBeNull();
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("An error occurred while processing your request");
        }
    }
}
