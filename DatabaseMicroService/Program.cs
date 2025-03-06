using ApplicationLayer.Interfaces;
using ApplicationLayer.Services;
using Azure.Identity;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Health;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
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
            dbConnectionString = builder.Configuration.GetConnectionString("ElectricityPriceDataContext");

            // Fetch connection string from Key Vault in non-development environments
            //var keyVaultManager = builder.Services.BuildServiceProvider().GetRequiredService<IKeyVaultSecretManager>();
            //var vaultSecret = await keyVaultManager.GetSecretAsync();
            //dbConnectionString = vaultSecret.DbConnectionString;
        }

        // Register the DbContext with the appropriate connection string
        builder.Services.AddDbContext<ElectricityDbContext>(options =>
            options.UseSqlServer(dbConnectionString));

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

        // Configure the HTTP request pipeline
        if (builder.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            var keyVaultUrl = builder.Configuration["KeyVault:BaseUrl"];
            builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), new DefaultAzureCredential());
        }

        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Application starting up");

        //app.Lifetime.ApplicationStarted.Register(() => logger.LogInformation("Application started"));
        app.Lifetime.ApplicationStarted.Register(async () =>
        {
            logger.LogInformation("Application started");

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




//this is dev code/////

//using ApplicationLayer.Interfaces;
//using ApplicationLayer.Services;
//using Domain.Interfaces;
//using Infrastructure.Data;
//using Infrastructure.Health;
//using Infrastructure.Repositories;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.OpenApi.Models;
//using System.Text.Json.Serialization;

//public class Program
//{
//    public static async Task Main(string[] args)
//    {
//        var builder = WebApplication.CreateBuilder(args);

//        // Register services needed for application setup
//        builder.Services.AddControllers()
//            .AddJsonOptions(options =>
//            {
//                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
//                options.JsonSerializerOptions.PropertyNamingPolicy = null;
//            });

//        builder.Services.AddEndpointsApiExplorer();
//        builder.Services.AddHttpClient();
//        builder.Services.AddApplicationInsightsTelemetry();

//        builder.Services.AddSwaggerGen(c =>
//        {
//            c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
//        });

//        builder.Configuration
//            .SetBasePath(Directory.GetCurrentDirectory())
//            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
//            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
//            .AddEnvironmentVariables()
//            .AddUserSecrets<Program>();


//        //uncomment when local dev and also uncomment tests
//        // Get connection string directly from configuration
//        var dbConnectionString = builder.Configuration.GetConnectionString("ElectricityPriceDataContext");

//        // Register the DbContext with the connection string
//        builder.Services.AddDbContext<ElectricityDbContext>(options =>
//            options.UseSqlServer(dbConnectionString));

//        // Service registrations
//        builder.Services.AddScoped<IElectrictyService, ElectrictyService>();
//        builder.Services.AddScoped<IElectricityRepository, ElectricityRepository>();
//        builder.Services.AddScoped<ISaveHistoryDataService, SaveHistoryDataService>();
//        builder.Services.AddScoped<IDateRangeDataService, DateRangeDataService>();
//        builder.Services.AddScoped<ICsvReaderService, CsvReaderService>();
//        builder.Services.AddScoped<IElectricityPriceService, ElectricityPriceService>();
//        builder.Services.AddScoped<IConsumptionDataProcessor, ConsumptionDataProcessor>();
//        builder.Services.AddScoped<IConsumptionOptimizer, ConsumptionOptimizer>();
//        builder.Services.AddScoped<ICalculateFingridConsumptionPrice, CalculateFinGridConsumptionPriceService>();

//        // Hosted Services
//        builder.Services.AddHostedService<DataLoaderHostedService>();
//        builder.Services.AddHostedService<ElectricityPriceFetchingBackgroundService>();

//        builder.Services.AddMemoryCache();

//        // Health checks setup
//        builder.Services.AddHttpClient();
//        builder.Services.AddHealthChecks()
//            .AddCheck<ElectricityServiceHealthCheck>("ElectricityServiceHealthCheck");

//        // Build the app
//        var app = builder.Build();

//        // // Configure the HTTP request pipeline
//        // if (builder.Environment.IsDevelopment())
//        // {
//        //     app.UseSwagger();
//        //     app.UseSwaggerUI();
//        // }
//        app.UseSwagger();
//        app.UseSwaggerUI();

//        var logger = app.Services.GetRequiredService<ILogger<Program>>();
//        logger.LogInformation("Application starting up");

//        app.Lifetime.ApplicationStarted.Register(async () =>
//        {
//            logger.LogInformation("Application started");

//            // Resolve the IElectricityRepository service
//            using (var scope = app.Services.CreateScope())
//            {
//                var electricityRepository = scope.ServiceProvider.GetRequiredService<IElectricityRepository>();

//                // Set the startDate to today and the endDate to 10 years ago
//                DateTime startDate = DateTime.Today.AddYears(-10);
//                DateTime endDate = DateTime.Today;
//                endDate = endDate.AddHours(23).AddHours(23).AddMinutes(59).AddSeconds(59);

//                // Make the call to GetPricesForPeriodAsync
//                var prices = await electricityRepository.GetPricesForPeriodAsync(startDate, endDate);

//                // Log the result
//                logger.LogInformation($"Fetched {prices.Count()} prices for the period from {endDate} to {startDate}");
//            }
//        });
//        app.Lifetime.ApplicationStopping.Register(() => logger.LogInformation("Application stopping"));
//        app.Lifetime.ApplicationStopped.Register(() => logger.LogInformation("Application stopped"));

//        app.UseCors(options => options
//            .AllowAnyOrigin()
//            .AllowAnyMethod()
//            .AllowAnyHeader());

//        app.UseHttpsRedirection();

//        app.MapControllers();
//        app.Run();
//    }
//}