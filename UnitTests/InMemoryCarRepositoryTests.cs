using dissertation_test_repo.Models;
using dissertation_test_repo.Repositories;
using NUnit.Framework;
using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture]
    public class InMemoryCarRepositoryTests
    {
        private InMemoryCarRepository _repository;

        [SetUp]
        public void Setup()
        {
            _repository = new InMemoryCarRepository();
        }

        #region Constructor and SeedData Tests

        [Test]
        public void Constructor_InitializesWithSeedData()
        {
            // Act
            var cars = _repository.GetAllAsync().Result;

            // Assert
            cars.Should().HaveCount(5);
            cars.Should().Contain(car => car.Make == "Toyota" && car.Model == "Camry");
            cars.Should().Contain(car => car.Make == "Honda" && car.Model == "Civic");
            cars.Should().Contain(car => car.Make == "Ford" && car.Model == "Mustang");
            cars.Should().Contain(car => car.Make == "BMW" && car.Model == "X3");
            cars.Should().Contain(car => car.Make == "Audi" && car.Model == "A4");
        }

        [Test]
        public void Constructor_SeedsDataWithIncrementalIds()
        {
            // Act
            var cars = _repository.GetAllAsync().Result.OrderBy(c => c.Id);

            // Assert
            var carList = cars.ToList();
            carList[0].Id.Should().Be(1);
            carList[1].Id.Should().Be(2);
            carList[2].Id.Should().Be(3);
            carList[3].Id.Should().Be(4);
            carList[4].Id.Should().Be(5);
        }

        [Test]
        public void Constructor_SeedsDataWithCorrectAvailability()
        {
            // Act
            var cars = _repository.GetAllAsync().Result;

            // Assert
            var availableCars = cars.Where(c => c.IsAvailable).ToList();
            var unavailableCars = cars.Where(c => !c.IsAvailable).ToList();

            availableCars.Should().HaveCount(4);
            unavailableCars.Should().HaveCount(1);
            unavailableCars.First().Make.Should().Be("Ford");
            unavailableCars.First().Model.Should().Be("Mustang");
        }

        #endregion

        #region GetAllAsync Tests

        [Test]
        public async Task GetAllAsync_ReturnsAllCars_IncludingSeedData()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(5);
            result.Should().AllSatisfy(car => car.Id.Should().BeGreaterThan(0));
        }

        [Test]
        public async Task GetAllAsync_ReturnsCorrectCarDetails()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            var toyotaCamry = result.First(c => c.Make == "Toyota");
            toyotaCamry.Model.Should().Be("Camry");
            toyotaCamry.Year.Should().Be(2022);
            toyotaCamry.Color.Should().Be("Silver");
            toyotaCamry.Price.Should().Be(28000);
            toyotaCamry.IsAvailable.Should().BeTrue();
        }

        #endregion

        #region GetByIdAsync Tests

        [Test]
        public async Task GetByIdAsync_ReturnsCar_WhenCarExists()
        {
            // Arrange
            var existingCars = await _repository.GetAllAsync();
            var firstCar = existingCars.First();

            // Act
            var result = await _repository.GetByIdAsync(firstCar.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(firstCar.Id);
            result.Make.Should().Be(firstCar.Make);
            result.Model.Should().Be(firstCar.Model);
        }

        [Test]
        public async Task GetByIdAsync_ReturnsNull_WhenCarDoesNotExist()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task GetByIdAsync_ReturnsNull_WhenIdIsZero()
        {
            // Act
            var result = await _repository.GetByIdAsync(0);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task GetByIdAsync_ReturnsNull_WhenIdIsNegative()
        {
            // Act
            var result = await _repository.GetByIdAsync(-1);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateAsync Tests

        [Test]
        public async Task CreateAsync_CreatesNewCar_WithIncrementalId()
        {
            // Arrange
            var newCar = new Car
            {
                Make = "Tesla",
                Model = "Model 3",
                Year = 2023,
                Color = "White",
                Price = 50000,
                IsAvailable = true
            };

            // Act
            var result = await _repository.CreateAsync(newCar);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(6); // Should be next after seed data (1-5)
            result.Make.Should().Be("Tesla");
            result.Model.Should().Be("Model 3");
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Test]
        public async Task CreateAsync_AddsCarToRepository()
        {
            // Arrange
            var newCar = new Car
            {
                Make = "Tesla",
                Model = "Model S",
                Year = 2023,
                Color = "Black",
                Price = 80000,
                IsAvailable = true
            };

            // Act
            var createdCar = await _repository.CreateAsync(newCar);
            var allCars = await _repository.GetAllAsync();

            // Assert
            allCars.Should().HaveCount(6); // 5 seed + 1 new
            allCars.Should().Contain(car => car.Id == createdCar.Id);
        }

        [Test]
        public async Task CreateAsync_SetsCreatedAtToUtcNow()
        {
            // Arrange
            var newCar = new Car
            {
                Make = "Nissan",
                Model = "Altima",
                Year = 2023,
                Color = "Gray",
                Price = 30000,
                IsAvailable = true
            };
            var beforeCreate = DateTime.UtcNow;

            // Act
            var result = await _repository.CreateAsync(newCar);

            // Assert
            var afterCreate = DateTime.UtcNow;
            result.CreatedAt.Should().BeOnOrAfter(beforeCreate);
            result.CreatedAt.Should().BeOnOrBefore(afterCreate);
        }

        [Test]
        public async Task CreateAsync_AssignsUniqueIds_ForMultipleCars()
        {
            // Arrange
            var car1 = new Car { Make = "Car1", Model = "Model1", Year = 2023, Color = "Red", Price = 10000, IsAvailable = true };
            var car2 = new Car { Make = "Car2", Model = "Model2", Year = 2023, Color = "Blue", Price = 20000, IsAvailable = true };

            // Act
            var result1 = await _repository.CreateAsync(car1);
            var result2 = await _repository.CreateAsync(car2);

            // Assert
            result1.Id.Should().NotBe(result2.Id);
            result2.Id.Should().Be(result1.Id + 1);
        }

        #endregion

        #region UpdateAsync Tests

        [Test]
        public async Task UpdateAsync_UpdatesExistingCar_WhenCarExists()
        {
            // Arrange
            var existingCars = await _repository.GetAllAsync();
            var carToUpdate = existingCars.First();
            var originalCreatedAt = carToUpdate.CreatedAt;

            var updatedCar = new Car
            {
                Make = "Updated Make",
                Model = "Updated Model",
                Year = 2024,
                Color = "Updated Color",
                Price = 99999,
                IsAvailable = false
            };

            // Act
            var result = await _repository.UpdateAsync(carToUpdate.Id, updatedCar);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(carToUpdate.Id);
            result.Make.Should().Be("Updated Make");
            result.Model.Should().Be("Updated Model");
            result.Year.Should().Be(2024);
            result.Color.Should().Be("Updated Color");
            result.Price.Should().Be(99999);
            result.IsAvailable.Should().BeFalse();
            result.CreatedAt.Should().Be(originalCreatedAt); // Should preserve original CreatedAt
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Test]
        public async Task UpdateAsync_ReturnsNull_WhenCarDoesNotExist()
        {
            // Arrange
            var updatedCar = new Car
            {
                Make = "Non-existent",
                Model = "Car",
                Year = 2023,
                Color = "Green",
                Price = 50000,
                IsAvailable = true
            };

            // Act
            var result = await _repository.UpdateAsync(999, updatedCar);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task UpdateAsync_PersistsChangesInRepository()
        {
            // Arrange
            var existingCars = await _repository.GetAllAsync();
            var carToUpdate = existingCars.First();
            var updatedCar = new Car
            {
                Make = "Persistent Update",
                Model = carToUpdate.Model,
                Year = carToUpdate.Year,
                Color = carToUpdate.Color,
                Price = carToUpdate.Price,
                IsAvailable = carToUpdate.IsAvailable
            };

            // Act
            await _repository.UpdateAsync(carToUpdate.Id, updatedCar);
            var retrievedCar = await _repository.GetByIdAsync(carToUpdate.Id);

            // Assert
            retrievedCar.Should().NotBeNull();
            retrievedCar.Make.Should().Be("Persistent Update");
        }

        [Test]
        public async Task UpdateAsync_SetsUpdatedAtToUtcNow()
        {
            // Arrange
            var existingCars = await _repository.GetAllAsync();
            var carToUpdate = existingCars.First();
            var updatedCar = new Car
            {
                Make = carToUpdate.Make,
                Model = carToUpdate.Model,
                Year = carToUpdate.Year,
                Color = carToUpdate.Color,
                Price = carToUpdate.Price,
                IsAvailable = carToUpdate.IsAvailable
            };
            var beforeUpdate = DateTime.UtcNow;

            // Act
            var result = await _repository.UpdateAsync(carToUpdate.Id, updatedCar);

            // Assert
            var afterUpdate = DateTime.UtcNow;
            result.UpdatedAt.Should().BeOnOrAfter(beforeUpdate);
            result.UpdatedAt.Should().BeOnOrBefore(afterUpdate);
        }

        #endregion

        #region DeleteAsync Tests

        [Test]
        public async Task DeleteAsync_ReturnsTrue_WhenCarExistsAndIsDeleted()
        {
            // Arrange
            var existingCars = await _repository.GetAllAsync();
            var carToDelete = existingCars.First();

            // Act
            var result = await _repository.DeleteAsync(carToDelete.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task DeleteAsync_RemovesCarFromRepository()
        {
            // Arrange
            var existingCars = await _repository.GetAllAsync();
            var carToDelete = existingCars.First();
            var originalCount = existingCars.Count();

            // Act
            await _repository.DeleteAsync(carToDelete.Id);
            var remainingCars = await _repository.GetAllAsync();
            var deletedCar = await _repository.GetByIdAsync(carToDelete.Id);

            // Assert
            remainingCars.Should().HaveCount(originalCount - 1);
            deletedCar.Should().BeNull();
            remainingCars.Should().NotContain(car => car.Id == carToDelete.Id);
        }

        [Test]
        public async Task DeleteAsync_ReturnsFalse_WhenCarDoesNotExist()
        {
            // Act
            var result = await _repository.DeleteAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task DeleteAsync_ReturnsFalse_WhenIdIsZero()
        {
            // Act
            var result = await _repository.DeleteAsync(0);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task DeleteAsync_ReturnsFalse_WhenIdIsNegative()
        {
            // Act
            var result = await _repository.DeleteAsync(-1);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region GetByMakeAsync Tests

        [Test]
        public async Task GetByMakeAsync_ReturnsMatchingCars_WhenCarsWithMakeExist()
        {
            // Act
            var result = await _repository.GetByMakeAsync("Toyota");

            // Assert
            result.Should().HaveCount(1);
            var toyotaCar = result.First();
            toyotaCar.Make.Should().Be("Toyota");
            toyotaCar.Model.Should().Be("Camry");
        }

        [Test]
        public async Task GetByMakeAsync_IsCaseInsensitive()
        {
            // Act
            var lowerCaseResult = await _repository.GetByMakeAsync("toyota");
            var upperCaseResult = await _repository.GetByMakeAsync("TOYOTA");
            var mixedCaseResult = await _repository.GetByMakeAsync("ToYoTa");

            // Assert
            lowerCaseResult.Should().HaveCount(1);
            upperCaseResult.Should().HaveCount(1);
            mixedCaseResult.Should().HaveCount(1);
            lowerCaseResult.First().Make.Should().Be("Toyota");
            upperCaseResult.First().Make.Should().Be("Toyota");
            mixedCaseResult.First().Make.Should().Be("Toyota");
        }

        [Test]
        public async Task GetByMakeAsync_ReturnsEmptyCollection_WhenNoMatchingCars()
        {
            // Act
            var result = await _repository.GetByMakeAsync("Nonexistent");

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetByMakeAsync_ReturnsMultipleCars_WhenMultipleCarsWithSameMakeExist()
        {
            // Arrange - Add another Toyota
            var newToyota = new Car
            {
                Make = "Toyota",
                Model = "Prius",
                Year = 2023,
                Color = "Green",
                Price = 30000,
                IsAvailable = true
            };
            await _repository.CreateAsync(newToyota);

            // Act
            var result = await _repository.GetByMakeAsync("Toyota");

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(car => car.Make.Should().Be("Toyota"));
            result.Should().Contain(car => car.Model == "Camry");
            result.Should().Contain(car => car.Model == "Prius");
        }

        [Test]
        public async Task GetByMakeAsync_ReturnsEmptyCollection_WhenMakeIsNull()
        {
            // Act
            var result = await _repository.GetByMakeAsync(null);

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetByMakeAsync_ReturnsEmptyCollection_WhenMakeIsEmpty()
        {
            // Act
            var result = await _repository.GetByMakeAsync("");

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetAvailableCarsAsync Tests

        [Test]
        public async Task GetAvailableCarsAsync_ReturnsOnlyAvailableCars()
        {
            // Act
            var result = await _repository.GetAvailableCarsAsync();

            // Assert
            result.Should().HaveCount(4); // Ford Mustang is not available in seed data
            result.Should().AllSatisfy(car => car.IsAvailable.Should().BeTrue());
            result.Should().NotContain(car => car.Make == "Ford" && car.Model == "Mustang");
        }

        [Test]
        public async Task GetAvailableCarsAsync_ReturnsCorrectAvailableCars()
        {
            // Act
            var result = await _repository.GetAvailableCarsAsync();

            // Assert
            var resultList = result.ToList();
            resultList.Should().Contain(car => car.Make == "Toyota" && car.Model == "Camry");
            resultList.Should().Contain(car => car.Make == "Honda" && car.Model == "Civic");
            resultList.Should().Contain(car => car.Make == "BMW" && car.Model == "X3");
            resultList.Should().Contain(car => car.Make == "Audi" && car.Model == "A4");
        }

        [Test]
        public async Task GetAvailableCarsAsync_ReflectsChangesAfterUpdate()
        {
            // Arrange - Make Ford Mustang available
            var fordMustang = (await _repository.GetAllAsync()).First(c => c.Make == "Ford");
            fordMustang.IsAvailable = true;
            await _repository.UpdateAsync(fordMustang.Id, fordMustang);

            // Act
            var result = await _repository.GetAvailableCarsAsync();

            // Assert
            result.Should().HaveCount(5); // Now all cars should be available
            result.Should().Contain(car => car.Make == "Ford" && car.Model == "Mustang");
        }

        [Test]
        public async Task GetAvailableCarsAsync_ReflectsChangesAfterMakingCarUnavailable()
        {
            // Arrange - Make Toyota unavailable
            var toyota = (await _repository.GetAllAsync()).First(c => c.Make == "Toyota");
            toyota.IsAvailable = false;
            await _repository.UpdateAsync(toyota.Id, toyota);

            // Act
            var result = await _repository.GetAvailableCarsAsync();

            // Assert
            result.Should().HaveCount(3); // Should have 3 available cars now
            result.Should().NotContain(car => car.Make == "Toyota");
        }

        [Test]
        public async Task GetAvailableCarsAsync_ReturnsEmptyCollection_WhenNoCarsAreAvailable()
        {
            // Arrange - Make all cars unavailable
            var allCars = await _repository.GetAllAsync();
            foreach (var car in allCars)
            {
                car.IsAvailable = false;
                await _repository.UpdateAsync(car.Id, car);
            }

            // Act
            var result = await _repository.GetAvailableCarsAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region Integration Tests

        [Test]
        public async Task Repository_MaintainsDataConsistency_ThroughMultipleOperations()
        {
            // Arrange & Act
            var initialCount = (await _repository.GetAllAsync()).Count();

            // Create a new car
            var newCar = new Car { Make = "Integration", Model = "Test", Year = 2023, Color = "Purple", Price = 1000, IsAvailable = true };
            var createdCar = await _repository.CreateAsync(newCar);

            // Update the car
            createdCar.Make = "Updated Integration";
            var updatedCar = await _repository.UpdateAsync(createdCar.Id, createdCar);

            // Verify consistency
            var retrievedCar = await _repository.GetByIdAsync(createdCar.Id);
            var allCars = await _repository.GetAllAsync();

            // Assert
            allCars.Should().HaveCount(initialCount + 1);
            retrievedCar.Should().NotBeNull();
            retrievedCar.Make.Should().Be("Updated Integration");
            updatedCar.Make.Should().Be("Updated Integration");
        }

        #endregion
    }
}