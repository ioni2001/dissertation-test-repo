using dissertation_test_repo.DTOs;
using dissertation_test_repo.Services;
using Microsoft.AspNetCore.Mvc;

namespace dissertation_test_repo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarsController : ControllerBase
    {
        private readonly ICarService _carService;
        private readonly ILogger<CarsController> _logger;

        public CarsController(ICarService carService, ILogger<CarsController> logger)
        {
            _carService = carService;
            _logger = logger;
        }

        /// <summary>
        /// Get all cars
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CarResponseDto>>> GetAllCars()
        {
            try
            {
                var cars = await _carService.GetAllCarsAsync();
                return Ok(cars);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all cars");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Get a specific car by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CarResponseDto>> GetCarById(int id)
        {
            try
            {
                var car = await _carService.GetCarByIdAsync(id);
                if (car == null)
                {
                    return NotFound($"Car with ID {id} not found");
                }
                return Ok(car);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting car with ID: {CarId}", id);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Create a new car
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CarResponseDto>> CreateCar([FromBody] CarCreateDto carDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdCar = await _carService.CreateCarAsync(carDto);
                return CreatedAtAction(nameof(GetCarById), new { id = createdCar.Id }, createdCar);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating car");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Update an existing car
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<CarResponseDto>> UpdateCar(int id, [FromBody] CarUpdateDto carDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedCar = await _carService.UpdateCarAsync(id, carDto);
                if (updatedCar == null)
                {
                    return NotFound($"Car with ID {id} not found");
                }
                return Ok(updatedCar);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating car with ID: {CarId}", id);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Delete a car
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCar(int id)
        {
            try
            {
                var deleted = await _carService.DeleteCarAsync(id);
                if (!deleted)
                {
                    return NotFound($"Car with ID {id} not found");
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting car with ID: {CarId}", id);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Get cars by make
        /// </summary>
        [HttpGet("make/{make}")]
        public async Task<ActionResult<IEnumerable<CarResponseDto>>> GetCarsByMake(string make)
        {
            try
            {
                var cars = await _carService.GetCarsByMakeAsync(make);
                return Ok(cars);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting cars by make: {Make}", make);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }
    }
}
