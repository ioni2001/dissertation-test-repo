using dissertation_test_repo.Models;
using dissertation_test_repo.Repositories;
using NUnit.Framework;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;

namespace dissertation_test_repo.Tests.Repositories
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

        [Test]
        public async Task GetAvailableCarsAsync_ReturnsOnlyAvailableCars()
        {
            // Arrange & Act
            var availableCars = await _repository.GetAvailableCarsAsync();

            // Assert
            availableCars.Should().AllSatisfy(car => car.IsAvailable.Should().BeTrue());
            availableCars.Count().Should().BeGreaterThan(0);
        }
    }
}
