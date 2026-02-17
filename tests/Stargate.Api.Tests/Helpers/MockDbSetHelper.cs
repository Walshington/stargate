using Microsoft.EntityFrameworkCore;
using Moq;

namespace Stargate.Api.Tests.Helpers
{
    /// <summary>
    /// Helper utilities for mocking EF Core DbSet with Moq.
    /// Simplifies setup of IQueryable interface for LINQ queries.
    /// </summary>
    public static class MockDbSetHelper
    {
        /// <summary>
        /// Creates a mock DbSet from a list of data.
        /// Supports synchronous LINQ queries like .Any(), .FirstOrDefault(), .Where(), etc.
        /// </summary>
        public static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();

            // Setup IQueryable interface for LINQ support
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            return mockSet;
        }

        /// <summary>
        /// Creates an empty mock DbSet (useful for new entities where no existing data needed).
        /// </summary>
        public static Mock<DbSet<T>> CreateEmptyMockDbSet<T>() where T : class
        {
            return CreateMockDbSet(new List<T>());
        }
    }
}
