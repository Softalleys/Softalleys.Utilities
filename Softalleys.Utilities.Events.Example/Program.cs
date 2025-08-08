using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Softalleys.Utilities.Events;
using Softalleys.Utilities.Events.Example;

// Create host with dependency injection
var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        // Register the event system with automatic handler discovery
        services.AddSoftalleysEvents(typeof(Program).Assembly);
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var eventBus = host.Services.GetRequiredService<IEventBus>();

logger.LogInformation("🚀 Softalleys.Utilities.Events Example Application Started");
logger.LogInformation("");

try
{
    // Demonstrate the event system with a user registration event
    logger.LogInformation("📋 Publishing UserRegisteredEvent...");
    logger.LogInformation("Expected execution order:");
    logger.LogInformation("1. Pre-Singleton Handlers");
    logger.LogInformation("2. Pre-Scoped Handlers");
    logger.LogInformation("3. Main-Singleton Handlers");
    logger.LogInformation("4. Main-Scoped Handlers (concurrent)");
    logger.LogInformation("5. Post-Singleton Handlers");
    logger.LogInformation("6. Post-Scoped Handlers");
    logger.LogInformation("");

    var userRegisteredEvent = new UserRegisteredEvent
    {
        UserId = "USER-12345",
        Email = "john.doe@example.com",
        RegisteredAt = DateTime.UtcNow
    };

    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    await eventBus.PublishAsync(userRegisteredEvent);
    
    stopwatch.Stop();
    
    logger.LogInformation("");
    logger.LogInformation("✅ UserRegisteredEvent processing completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
    logger.LogInformation("");

    // Demonstrate with an order event
    logger.LogInformation("📋 Publishing OrderCreatedEvent...");
    logger.LogInformation("");

    var orderCreatedEvent = new OrderCreatedEvent
    {
        OrderId = "ORDER-67890",
        Amount = 149.99m,
        CustomerId = "CUSTOMER-54321"
    };

    stopwatch.Restart();
    
    await eventBus.PublishAsync(orderCreatedEvent);
    
    stopwatch.Stop();
    
    logger.LogInformation("");
    logger.LogInformation("✅ OrderCreatedEvent processing completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
    logger.LogInformation("");

    // Demonstrate error handling
    logger.LogInformation("📋 Testing error handling with invalid event...");
    logger.LogInformation("");

    var invalidUserEvent = new UserRegisteredEvent
    {
        UserId = "USER-ERROR",
        Email = "", // This will cause validation to fail
        RegisteredAt = DateTime.UtcNow
    };

    try
    {
        await eventBus.PublishAsync(invalidUserEvent);
    }
    catch (AggregateException ex)
    {
        logger.LogWarning("⚠️ Expected error occurred during event processing:");
        foreach (var innerEx in ex.InnerExceptions)
        {
            logger.LogWarning("   - {ErrorMessage}", innerEx.Message);
        }
    }
    
    logger.LogInformation("");
    logger.LogInformation("🎉 Example completed successfully!");
}
catch (Exception ex)
{
    logger.LogError(ex, "❌ An unexpected error occurred");
}

logger.LogInformation("");
logger.LogInformation("Press any key to exit...");
Console.ReadKey();
