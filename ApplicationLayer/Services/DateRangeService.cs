using ApplicationLayer.Interfaces;
using Domain.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ApplicationLayer.Services
{
    public class DateRangeDataService : IDateRangeDataService
    {
        private readonly IElectricityRepository _electricityRepository;
        private readonly ILogger<DateRangeDataService> _logger;

        public DateRangeDataService(IElectricityRepository electricityRepository, ILogger<DateRangeDataService> logger)
        {
            _electricityRepository = electricityRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<ElectricityPriceData>> GetPricesForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            _logger.LogInformation("GetPricesForPeriodAsync called with startDate: {StartDate}, endDate: {EndDate}", startDate, endDate);

            try
            {
                IEnumerable<ElectricityPriceData> prices = await _electricityRepository.GetPricesForPeriodAsync(startDate, endDate);
                var paddedPrices = PadMissingTimes(prices.ToList(), startDate, endDate);

                _logger.LogInformation("Electricity prices retrieved successfully for period: {StartDate} - {EndDate}.", startDate, endDate);
                return paddedPrices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving electricity prices for period: {StartDate} - {EndDate}", startDate, endDate);
                throw;
            }
        }

        private List<ElectricityPriceData> PadMissingTimes(List<ElectricityPriceData> prices, DateTime startDate, DateTime endDate)
        {
            var existingTimes = new HashSet<DateTime>(prices.Select(p => p.StartDate));
            var paddedTimes = new List<ElectricityPriceData>();

            for (var date = startDate.Date; date < endDate.Date; date = date.AddDays(1))
            {
                for (var hour = 0; hour < 24; hour++)
                {
                    var time = DateTime.SpecifyKind(date.AddHours(hour), DateTimeKind.Unspecified);
                    if (!existingTimes.Contains(time))
                    {
                        paddedTimes.Add(new ElectricityPriceData
                        {
                            StartDate = time,
                            EndDate = time.AddHours(1),
                            Price = 0 // or any default value
                        });
                    }
                }
            }

            paddedTimes.AddRange(prices);
            return paddedTimes.OrderBy(p => p.StartDate).ToList();
        }
    }
}
