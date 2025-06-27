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
using Microsoft.AspNetCore.Http;

namespace dissertation_test_repo.Tests.Controllers
{
    [TestFixture]
    public class CarsControllerTests
    {
        private ICarService _carService;
        private ILogger<CarsController> _logger;
        private CarsController _carsController;

        [SetUp]
        public void Setup()
        {
            _carService = Substitute.For<ICarService>();
            _logger = Substitute.For<ILogger<CarsController>>();
            _carsController = new CarsController(_carService, _logger);
        }

        [Test]
        public async Task GetAvailableCars_ReturnsOkResultWithCars()
        {
            // Arrange
            var cars = new List<CarResponseDto> { new CarResponseDto { Id = 1, Make = "Toyota" } };
            _carService.GetAvailableCarsAsync().Returns(Task.FromResult<IEnumerable<CarResponseDto>>(cars));

            // Act
            var result = await _carsController.GetAvailableCars() as ActionResult<IEnumerable<CarResponseDto>>;

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
            var result = await _carsController.GetAvailableCars();
            var objectResult = result.Result as ObjectResult;

            // Assert
            objectResult.Should().NotBeNull();
            objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            objectResult.Value.Should().Be("An error occurred while processing your request");
            _logger.Received(1).LogError(Arg.Any<Exception>(), "Error occurred while getting available cars");
        }

        [Test]
        public async Task MarkAsUnavailable_CarExists_ReturnsOkResultWithCar()
        {
            // Arrange
            var car = new CarResponseDto { Id = 1, Make = "Toyota", IsAvailable = false };
            _carService.MarkAsUnavailableAsync(1).Returns(Task.FromResult(car));

            // Act
            var result = await _carsController.MarkAsUnavailable(1) as ActionResult<CarResponseDto>;

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(car);
        }

        [Test]
        public async Task MarkAsUnavailable_CarDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            _carService.MarkAsUnavailableAsync(1).Returns((Task<CarResponseDto>)Task.FromResult<CarResponseDto>(null));

            // Act
            var result = await _carsController.MarkAsUnavailable(1) as ActionResult<CarResponseDto>;

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>()
                .Which.Value.Should().Be("Car with ID 1 not found");
        }

        [Test]
        public async Task MarkAsUnavailable_ExceptionThrown_ReturnsStatusCode500()
        {
            // Arrange
            _carService.MarkAsUnavailableAsync(1).Returns(Task.FromException<CarResponseDto>(new Exception("Test Exception")));

            // Act
            var result = await _carsController.MarkAsUnavailable(1);
            var objectResult = result.Result as ObjectResult;

            // Assert
            objectResult.Should().NotBeNull();
            objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            objectResult.Value.Should().Be("An error occurred while processing your request");
            _logger.Received(1).LogError(Arg.Any<Exception>(), "Error occurred while marking car as unavailable with ID: {CarId}", 1);
        }

        [Test]
        public async Task GetAveragePrice_ReturnsOkResultWithAveragePrice()
        {
            // Arrange
            double averagePrice = 25000;
            _carService.GetAveragePriceAsync().Returns(Task.FromResult(averagePrice));

            // Act
            var result = await _carsController.GetAveragePrice() as ActionResult<double>;

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be(averagePrice);
        }

        [Test]
        public async Task GetAveragePrice_ExceptionThrown_ReturnsStatusCode500()
        {
            // Arrange
            _carService.GetAveragePriceAsync().Returns(Task.FromException<double>(new Exception("Test Exception")));

            // Act
            var result = await _carsController.GetAveragePrice();
            var objectResult = result.Result as ObjectResult;

            // Assert
            objectResult.Should().NotBeNull();
            objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            objectResult.Value.Should().Be("An error occurred while processing your request");
            _logger.Received(1).LogError(Arg.Any<Exception>(), "Error occurred while calculating average price");
        }
    }
}