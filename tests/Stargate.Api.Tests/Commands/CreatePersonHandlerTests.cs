using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using Stargate.Api.Tests.Helpers;

namespace Stargate.Api.Tests.Commands
{
    public class CreatePersonHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldCreatePerson_WithValidRequest()
        {
            // Arrange
            var mockPeopleDbSet = MockDbSetHelper.CreateEmptyMockDbSet<Person>();

            var mockContext = new Mock<StargateContext>(
                new DbContextOptions<StargateContext>());
            mockContext.Setup(c => c.People).Returns(mockPeopleDbSet.Object);

            // Mock AddAsync to capture the added person
            Person? capturedPerson = null;
            mockPeopleDbSet
                .Setup(m => m.AddAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
                .Callback<Person, CancellationToken>((person, token) =>
                {
                    capturedPerson = person;
                    // Simulate database setting the Id
                    person.Id = 1;
                })
                .Returns((Person p, CancellationToken token) =>
                    new ValueTask<EntityEntry<Person>>(Task.FromResult<EntityEntry<Person>>(null!)));

            // Mock SaveChangesAsync
            mockContext
                .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new CreatePersonHandler(mockContext.Object);
            var request = new CreatePerson { Name = "John Doe" };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedPerson);
            Assert.Equal("John Doe", capturedPerson.Name);

            // Verify AddAsync was called
            mockPeopleDbSet.Verify(
                m => m.AddAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify SaveChangesAsync was called
            mockContext.Verify(
                c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnCreatedStatusCode()
        {
            // Arrange
            var mockPeopleDbSet = MockDbSetHelper.CreateEmptyMockDbSet<Person>();

            var mockContext = new Mock<StargateContext>(
                new DbContextOptions<StargateContext>());
            mockContext.Setup(c => c.People).Returns(mockPeopleDbSet.Object);

            // Mock AddAsync with callback to set Id
            mockPeopleDbSet
                .Setup(m => m.AddAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
                .Callback<Person, CancellationToken>((person, token) => person.Id = 1)
                .Returns((Person p, CancellationToken token) =>
                    new ValueTask<EntityEntry<Person>>(Task.FromResult<EntityEntry<Person>>(null!)));

            mockContext
                .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new CreatePersonHandler(mockContext.Object);
            var request = new CreatePerson { Name = "Jane Smith" };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal((int)HttpStatusCode.Created, result.ResponseCode);
            Assert.NotNull(result.Person);
            Assert.Equal("Jane Smith", result.Person.Name);
            Assert.Equal(1, result.Person.PersonId);
        }

        [Fact]
        public async Task Handle_ShouldReturnPersonAstronautDto_WithCorrectProperties()
        {
            // Arrange
            var mockPeopleDbSet = MockDbSetHelper.CreateEmptyMockDbSet<Person>();

            var mockContext = new Mock<StargateContext>(
                new DbContextOptions<StargateContext>());
            mockContext.Setup(c => c.People).Returns(mockPeopleDbSet.Object);

            mockPeopleDbSet
                .Setup(m => m.AddAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
                .Callback<Person, CancellationToken>((person, token) => person.Id = 42)
                .Returns((Person p, CancellationToken token) =>
                    new ValueTask<EntityEntry<Person>>(Task.FromResult<EntityEntry<Person>>(null!)));

            mockContext
                .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new CreatePersonHandler(mockContext.Object);
            var request = new CreatePerson { Name = "Test Person" };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Person);
            Assert.Equal(42, result.Person.PersonId);
            Assert.Equal("Test Person", result.Person.Name);
            Assert.Equal(string.Empty, result.Person.CurrentRank);
            Assert.Equal(string.Empty, result.Person.CurrentDutyTitle);
            Assert.Null(result.Person.CareerStartDate);
            Assert.Null(result.Person.CareerEndDate);
        }
    }
}
