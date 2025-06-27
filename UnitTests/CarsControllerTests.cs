using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using dissertation_test_repo.Controllers;
using dissertation_test_repo.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using dissertation_test_repo.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;

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
            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(cars);
        }

        [Test]
        public async Task GetAvailableCars_ReturnsStatusCode500_OnError()
        {
            // Arrange
            _carService.GetAvailableCarsAsync().Returns(Task.FromException<IEnumerable<CarResponseDto>>(new System.Exception("Test Exception")));

            // Act
            var result = await _controller.GetAvailableCars();

            // Assert
            result.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Test]
        public async Task MarkAsUnavailable_ReturnsOkResult_WhenCarIsUpdated()
        {
            // Arrange
            int carId = 1;
            var car = new CarResponseDto { Id = carId, Make = "Toyota", IsAvailable = false };
            _carService.MarkAsUnavailableAsync(carId).Returns(car);

            // Act
            var result = await _controller.MarkAsUnavailable(carId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(car);
        }

        [Test]
        public async Task MarkAsUnavailable_ReturnsNotFound_WhenCarDoesNotExist()
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
        public async Task MarkAsUnavailable_ReturnsStatusCode500_OnError()
        {
            // Arrange
            int carId = 1;
            _carService.MarkAsUnavailableAsync(carId).Returns(Task.FromException<CarResponseDto>(new System.Exception("Test Exception")));

            // Act
            var result = await _controller.MarkAsUnavailable(carId);

            // Assert
            result.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Test]
        public async Task GetAveragePrice_ReturnsOkResult_WithAveragePrice()
        {
            // Arrange
            double averagePrice = 25000.50;
            _carService.GetAveragePriceAsync().Returns(averagePrice);

            // Act
            var result = await _controller.GetAveragePrice();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be(averagePrice);
        }

        [Test]
        public async Task GetAveragePrice_ReturnsStatusCode500_OnError()
        {
            // Arrange
            _carService.GetAveragePriceAsync().Returns(Task.FromException<double>(new System.Exception("Test Exception")));

            // Act
            var result = await _controller.GetAveragePrice();

            // Assert
            result.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Test]
        public async Task DeleteCar_ReturnsNoContent_WhenCarIsDeleted()
        {
            // Arrange
            int carId = 1;
            _carService.DeleteCarAsync(carId).Returns(true);

            // Act
            var result = await _controller.DeleteCar(carId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Test]
        public async Task DeleteCar_ReturnsNotFound_WhenCarDoesNotExist()
        {
            // Arrange
            int carId = 1;
            _carService.DeleteCarAsync(carId).Returns(false);

            // Act
            var result = await _controller.DeleteCar(carId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Test]
        public async Task DeleteCar_ReturnsBadRequest_WhenCarIsNotAvailable()
        {
            // Arrange
            int carId = 1;
            _carService.DeleteCarAsync(carId).Returns(Task.FromException<bool>(new InvalidOperationException("Cannot delete a car that is not available.")));

            // Act
            var result = await _controller.DeleteCar(carId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task DeleteCar_ReturnsStatusCode500_OnError()
        {
            // Arrange
            int carId = 1;
            _carService.DeleteCarAsync(carId).Returns(Task.FromException<bool>(new System.Exception("Test Exception")));

            // Act
            var result = await _controller.DeleteCar(carId);

            // Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }
    }
}
