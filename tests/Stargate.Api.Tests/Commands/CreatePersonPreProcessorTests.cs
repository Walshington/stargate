using Moq;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Domain.Exceptions;
using Stargate.Api.Tests.Helpers;

namespace Stargate.Api.Tests.Commands
{
    public class CreatePersonPreProcessorTests
    {
        [Fact]
        public async Task Process_ShouldThrowException_WhenNameIsEmpty()
        {
            // Arrange
            var mockContext = new Mock<StargateContext>(
                new Microsoft.EntityFrameworkCore.DbContextOptions<StargateContext>());
            
            var mockPeopleDbSet = MockDbSetHelper.CreateEmptyMockDbSet<Person>();
            mockContext.Setup(c => c.People).Returns(mockPeopleDbSet.Object);

            var preprocessor = new CreatePersonPreProcessor(mockContext.Object);
            var request = new CreatePerson { Name = "" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnprocessableEntityException>(
                async () => await preprocessor.Process(request, CancellationToken.None)
            );

            Assert.Equal("Person name is required and cannot be empty.", exception.Message);
        }

        [Fact]
        public async Task Process_ShouldThrowException_WhenNameIsWhitespace()
        {
            // Arrange
            var mockContext = new Mock<StargateContext>(
                new Microsoft.EntityFrameworkCore.DbContextOptions<StargateContext>());
            
            var mockPeopleDbSet = MockDbSetHelper.CreateEmptyMockDbSet<Person>();
            mockContext.Setup(c => c.People).Returns(mockPeopleDbSet.Object);

            var preprocessor = new CreatePersonPreProcessor(mockContext.Object);
            var request = new CreatePerson { Name = "   " };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnprocessableEntityException>(
                async () => await preprocessor.Process(request, CancellationToken.None)
            );

            Assert.Equal("Person name is required and cannot be empty.", exception.Message);
        }

        [Fact]
        public async Task Process_ShouldThrowException_WhenPersonAlreadyExists()
        {
            // Arrange
            var existingPeople = new List<Person>
            {
                new Person { Id = 1, Name = "John Doe" }
            };

            var mockPeopleDbSet = MockDbSetHelper.CreateMockDbSet(existingPeople);
            
            var mockContext = new Mock<StargateContext>(
                new Microsoft.EntityFrameworkCore.DbContextOptions<StargateContext>());
            mockContext.Setup(c => c.People).Returns(mockPeopleDbSet.Object);

            var preprocessor = new CreatePersonPreProcessor(mockContext.Object);
            var request = new CreatePerson { Name = "john doe" }; // case-insensitive match

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnprocessableEntityException>(
                async () => await preprocessor.Process(request, CancellationToken.None)
            );

            Assert.Equal("A person with this name already exists.", exception.Message);
        }

        [Fact]
        public async Task Process_ShouldSucceed_WhenNameIsValidAndUnique()
        {
            // Arrange
            var existingPeople = new List<Person>
            {
                new Person { Id = 1, Name = "Jane Doe" }
            };

            var mockPeopleDbSet = MockDbSetHelper.CreateMockDbSet(existingPeople);
            
            var mockContext = new Mock<StargateContext>(
                new Microsoft.EntityFrameworkCore.DbContextOptions<StargateContext>());
            mockContext.Setup(c => c.People).Returns(mockPeopleDbSet.Object);

            var preprocessor = new CreatePersonPreProcessor(mockContext.Object);
            var request = new CreatePerson { Name = "John Smith" };

            // Act
            await preprocessor.Process(request, CancellationToken.None);

            // Assert - no exception thrown means success
            Assert.True(true);
        }
    }
}
