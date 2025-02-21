﻿using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;


namespace TestProject.Infrastructure
{
    public class ElectricityRepositoryTests
    {
        [Fact]
        public async Task AddRangeElectricityPricesAsync_ValidInput_ReturnsTrue()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ElectricityDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_AddRangeElectricityPricesAsync_ValidInput")
                .Options;

            var loggerMock = new Mock<ILogger<ElectricityRepository>>();
            var cacheMock = new Mock<IMemoryCache>();

            using (var context = new ElectricityDbContext(options))
            {
                var repository = new ElectricityRepository(context, loggerMock.Object, cacheMock.Object);

                var electricityPriceData = new List<ElectricityPriceData>
                {
                    new ElectricityPriceData { /* Initialize properties */ },
                    new ElectricityPriceData { /* Initialize properties */ },
                };

                // Act
                var result = await repository.AddRangeElectricityPricesAsync(electricityPriceData);

                // Assert
                Assert.True(result);
                Assert.True(context.ElectricityPriceDatas.Count() == 2);
            }
        }

        [Fact]
        public async Task AddRangeElectricityPricesAsync_ExceptionThrown_ReturnsFalse()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ElectricityDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_AddRangeElectricityPricesAsync_ExceptionThrown")
                .Options;

            var loggerMock = new Mock<ILogger<ElectricityRepository>>();
            var cacheMock = new Mock<IMemoryCache>();

            using (var context = new ElectricityDbContext(options))
            {
                var repository = new ElectricityRepository(context, loggerMock.Object, cacheMock.Object);

                // Create an empty list
                var electricityPriceData = new List<ElectricityPriceData>();

                // Act
                var result = await repository.AddRangeElectricityPricesAsync(electricityPriceData);

                // Assert
                Assert.False(result);
                Assert.True(context.ElectricityPriceDatas.Count() == 0);
            }
        }

        [Fact]
        public async Task IsDuplicateAsync_ShouldReturnTrue_WhenDuplicateExists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ElectricityDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_IsDuplicateAsync_ShouldReturnTrue")
                .Options;

            var loggerMock = new Mock<ILogger<ElectricityRepository>>();
            var cacheMock = new Mock<IMemoryCache>();

            using (var context = new ElectricityDbContext(options))
            {
                var repository = new ElectricityRepository(context, loggerMock.Object, cacheMock.Object);

                var startDate = new DateTime(2023, 1, 1);
                var endDate = new DateTime(2023, 1, 2);
                context.ElectricityPriceDatas.Add(new ElectricityPriceData { StartDate = startDate, EndDate = endDate });
                await context.SaveChangesAsync();

                // Act
                var result = await repository.IsDuplicateAsync(startDate, endDate);

                // Assert
                Assert.True(result);
            }
        }

        [Fact]
        public async Task IsDuplicateAsync_ShouldReturnFalse_WhenDuplicateDoesNotExist()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ElectricityDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_IsDuplicateAsync_ShouldReturnFalse")
                .Options;

            var loggerMock = new Mock<ILogger<ElectricityRepository>>();
            var cacheMock = new Mock<IMemoryCache>();

            using (var context = new ElectricityDbContext(options))
            {
                var repository = new ElectricityRepository(context, loggerMock.Object, cacheMock.Object);

                var startDate = new DateTime(2023, 1, 1);
                var endDate = new DateTime(2023, 1, 2);

                // Act
                var result = await repository.IsDuplicateAsync(startDate, endDate);

                // Assert
                Assert.False(result);
            }
        }



        //private readonly ITestOutputHelper _output;

        //public ElectricityRepositoryTests(ITestOutputHelper output)
        //{
        //    _output = output;
        //}


        //[Theory]
        //[InlineData("2025-01-01", "2025-01-02",  // First fetch dates
        //   "2025-01-01", "2025-01-04",    // Second fetch dates
        //   "2025-01-01", "2025-01-02")]   // Third fetch dates - original test case
        //[InlineData("2025-02-03", "2025-02-06",   // First fetch: endDate - 3 days
        //   "2025-02-04", "2025-02-06",    // Second fetch: endDate - 2 days
        //   "2025-02-04", "2025-02-05")]   // Third fetch: endDate - 1 day (and endDate is one day less)
        ////[InlineData("2025-03-01", "2025-03-04",   // Another test case following same pattern
        ////   "2025-03-02", "2025-03-04",
        ////   "2025-03-03", "2025-03-03")]
        //public async Task NoDuplicateDataFetched(
        //string firstStartDate, string firstEndDate,
        //string secondStartDate, string secondEndDate,
        //string thirdStartDate, string thirdEndDate)
        //{
        //    // Arrange
        //    var options = new DbContextOptionsBuilder<ElectricityDbContext>()
        //        .UseSqlServer("Server=localhost;Database=TempusElectrica;TrustServerCertificate=True;Integrated Security=True;Trusted_Connection=True;")
        //        .Options;
        //    var loggerMock = new Mock<ILogger<ElectricityRepository>>();
        //    var cache = new MemoryCache(new MemoryCacheOptions());

        //    using (var context = new ElectricityDbContext(options))
        //    {
        //        var repository = new ElectricityRepository(context, loggerMock.Object, cache);

        //        // First fetch
        //        var cachedElectricityData = await repository.GetPricesForPeriodAsync(
        //            DateTime.Parse(firstStartDate),
        //            DateTime.Parse(firstEndDate));
        //        _output.WriteLine("First fetch result:");
        //        foreach (var data in cachedElectricityData)
        //        {
        //            _output.WriteLine($"StartDate: {data.StartDate}, EndDate: {data.EndDate}, Price: {data.Price}");
        //        }
        //        Assert.NotEmpty(cachedElectricityData);
        //        var firstFetchCount = cachedElectricityData.Count();

        //        // Second fetch
        //        cachedElectricityData = await repository.GetPricesForPeriodAsync(
        //            DateTime.Parse(secondStartDate),
        //            DateTime.Parse(secondEndDate));
        //        _output.WriteLine("Second fetch result:");
        //        foreach (var data in cachedElectricityData)
        //        {
        //            _output.WriteLine($"StartDate: {data.StartDate}, EndDate: {data.EndDate}, Price: {data.Price}");
        //        }

        //        // Third fetch
        //        cachedElectricityData = await repository.GetPricesForPeriodAsync(
        //            DateTime.Parse(thirdStartDate),
        //            DateTime.Parse(thirdEndDate));
        //        _output.WriteLine("Third fetch result:");
        //        foreach (var data in cachedElectricityData)
        //        {
        //            _output.WriteLine($"StartDate: {data.StartDate}, EndDate: {data.EndDate}, Price: {data.Price}");
        //        }
        //        var lastFetchCount = cachedElectricityData.Count();

        //        Assert.InRange(lastFetchCount,0, 24);
        //    }
        //}



    }
}