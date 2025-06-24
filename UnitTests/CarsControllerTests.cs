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
            var carList = new List<CarResponseDto> { new CarResponseDto(), new CarResponseDto() };
            _carService.GetAvailableCarsAsync().Returns(carList);

            // Act
            var result = await _controller.GetAvailableCars();

            // Assert
            result.Should().BeOfType<ActionResult<IEnumerable<CarResponseDto>>>();
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(carList);
        }

        [Test]
        public async Task GetAvailableCars_ReturnsStatusCode500_WhenExceptionIsThrown()
        {
            // Arrange
            _carService.GetAvailableCarsAsync().Returns(Task.FromException<IEnumerable<CarResponseDto>>(new Exception("Test Exception")));

            // Act
            var result = await _controller.GetAvailableCars();

            // Assert
            result.Should().BeOfType<ActionResult<IEnumerable<CarResponseDto>>>();
            var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(500);
            statusCodeResult.Value.Should().Be("An error occurred while processing your request");
        }

        [Test]
        public async Task MarkAsUnavailable_ReturnsOkResult_WhenCarIsUpdated()
        {
            // Arrange
            int carId = 1;
            var carResponse = new CarResponseDto { Id = carId };
            _carService.MarkAsUnavailableAsync(carId).Returns(carResponse);

            // Act
            var result = await _controller.MarkAsUnavailable(carId);

            // Assert
            result.Should().BeOfType<ActionResult<CarResponseDto>>();
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(carResponse);
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
            result.Should().BeOfType<ActionResult<CarResponseDto>>();
            var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
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
            result.Should().BeOfType<ActionResult<CarResponseDto>>();
            var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(500);
            statusCodeResult.Value.Should().Be("An error occurred while processing your request");
        }

        [Test]
        public async Task GetAveragePrice_ReturnsOkResult_WithAveragePrice()
        {
            // Arrange
            decimal averagePrice = 30000;
            _carService.GetAveragePriceAsync().Returns(averagePrice);

            // Act
            var result = await _controller.GetAveragePrice();

            // Assert
            result.Should().BeOfType<ActionResult<decimal>>();
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { AveragePrice = averagePrice });
        }

        [Test]
        public async Task GetAveragePrice_ReturnsStatusCode500_WhenExceptionIsThrown()
        {
            // Arrange
            _carService.GetAveragePriceAsync().Returns(Task.FromException<decimal>(new Exception("Test Exception")));

            // Act
            var result = await _controller.GetAveragePrice();

            // Assert
            result.Should().BeOfType<ActionResult<decimal>>();
            var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(500);
            statusCodeResult.Value.Should().Be("An error occurred while processing your request");
        }
    }
}
