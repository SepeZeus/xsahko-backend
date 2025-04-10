using ApplicationLayer.Interfaces;
using ApplicationLayer.Services;
using Azure.Identity;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Health;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Register services needed for application setup
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHttpClient();
        builder.Services.AddApplicationInsightsTelemetry();
        builder.Services.AddSingleton<IKeyVaultSecretManager, KeyVaultSecretManager>();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
        });

        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>();

        string dbConnectionString;

        if (builder.Environment.IsDevelopment())
        {
            // Fetch connection string from appsettings.json in development
            dbConnectionString = builder.Configuration.GetConnectionString("ElectricityPriceDataContext");
        }
        else
        {
            var keyVaultManager = builder.Services.BuildServiceProvider().GetRequiredService<IKeyVaultSecretManager>();
            var vaultSecret = await keyVaultManager.GetSecretAsync();
            dbConnectionString = vaultSecret.DbConnectionString;
        }

        // Register the DbContext with the appropriate connection string
        builder.Services.AddDbContext<ElectricityDbContext>(options =>
        {
            options.UseMySql(dbConnectionString, ServerVersion.AutoDetect(dbConnectionString),
                mysqlOptions => mysqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null));
        });

        // Service registrations
        builder.Services.AddScoped<IElectrictyService, ElectrictyService>();
        builder.Services.AddScoped<IElectricityRepository, ElectricityRepository>();
        builder.Services.AddScoped<ISaveHistoryDataService, SaveHistoryDataService>();
        builder.Services.AddScoped<IDateRangeDataService, DateRangeDataService>();
        builder.Services.AddScoped<ICsvReaderService, CsvReaderService>();
        builder.Services.AddScoped<IElectricityPriceService, ElectricityPriceService>();
        builder.Services.AddScoped<IConsumptionDataProcessor, ConsumptionDataProcessor>();
        builder.Services.AddScoped<IConsumptionOptimizer, ConsumptionOptimizer>();
        builder.Services.AddScoped<ICalculateFingridConsumptionPrice, CalculateFinGridConsumptionPriceService>();

        // Hosted Services
        builder.Services.AddHostedService<DataLoaderHostedService>();
        builder.Services.AddHostedService<ElectricityPriceFetchingBackgroundService>();

        builder.Services.AddMemoryCache();

        // Health checks setup
        builder.Services.AddHttpClient();
        builder.Services.AddHealthChecks()
            .AddCheck<ElectricityServiceHealthCheck>("ElectricityServiceHealthCheck");

        // Build the app
        var app = builder.Build();


        app.UseSwagger();
        app.UseSwaggerUI();

        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Application starting up");

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<ElectricityDbContext>();
                // This will create the database if it doesn't exist
                // and apply any pending migrations
                context.Database.Migrate();

                logger.LogInformation("Database migration completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating the database");
            }
        }

        app.Lifetime.ApplicationStarted.Register(async () =>
        {
            logger.LogInformation("Application started");

            await Task.Delay(TimeSpan.FromSeconds(180));

            // Resolve the IElectricityRepository service
            using (var scope = app.Services.CreateScope())
            {
                var electricityRepository = scope.ServiceProvider.GetRequiredService<IElectricityRepository>();

                // Set the startDate to today and the endDate to 10 years ago
                DateTime startDate = DateTime.Today.AddYears(-10);
                DateTime endDate = DateTime.Today;
                endDate = endDate.AddHours(23).AddHours(23).AddMinutes(59).AddSeconds(59);

                // Make the call to GetPricesForPeriodAsync
                var prices = await electricityRepository.GetPricesForPeriodAsync(startDate, endDate);

                // Log the result
                logger.LogInformation($"Fetched {prices.Count()} prices for the period from {endDate} to {startDate}");
            }
        });
        app.Lifetime.ApplicationStopping.Register(() => logger.LogInformation("Application stopping"));
        app.Lifetime.ApplicationStopped.Register(() => logger.LogInformation("Application stopped"));

        app.UseCors(options => options
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

        app.UseHttpsRedirection();

        app.MapControllers();
        app.Run();
    }
}


//////the keyvault stuff may or may not be important who knows
