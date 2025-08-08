using Microsoft.Extensions.Logging;
using Softalleys.Utilities.Events;

namespace Softalleys.Utilities.Events.Example;

// Example handlers demonstrating different types and lifecycles

// Pre-processing singleton handler
public class UserValidationPreSingletonHandler : IEventPreSingletonHandler<UserRegisteredEvent>
{
    private readonly ILogger<UserValidationPreSingletonHandler> _logger;

    public UserValidationPreSingletonHandler(ILogger<UserValidationPreSingletonHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(UserRegisteredEvent eventData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔍 [Pre-Singleton] Validating user registration for {UserId}", eventData.UserId);
        
        // Simulate validation
        await Task.Delay(100, cancellationToken);
        
        if (string.IsNullOrEmpty(eventData.Email))
        {
            throw new ArgumentException("Email is required");
        }
        
        _logger.LogInformation("✅ [Pre-Singleton] User validation completed for {UserId}", eventData.UserId);
    }
}

// Pre-processing scoped handler
public class DatabasePreparationPreHandler : IEventPreHandler<UserRegisteredEvent>
{
    private readonly ILogger<DatabasePreparationPreHandler> _logger;

    public DatabasePreparationPreHandler(ILogger<DatabasePreparationPreHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(UserRegisteredEvent eventData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🗄️ [Pre-Scoped] Preparing database for user {UserId}", eventData.UserId);
        
        // Simulate database preparation
        await Task.Delay(150, cancellationToken);
        
        _logger.LogInformation("✅ [Pre-Scoped] Database preparation completed for {UserId}", eventData.UserId);
    }
}

// Main singleton handler
public class UserRegistrationLoggerSingletonHandler : IEventSingletonHandler<UserRegisteredEvent>
{
    private readonly ILogger<UserRegistrationLoggerSingletonHandler> _logger;

    public UserRegistrationLoggerSingletonHandler(ILogger<UserRegistrationLoggerSingletonHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(UserRegisteredEvent eventData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📝 [Main-Singleton] Logging user registration: {UserId} - {Email} at {RegisteredAt}", 
            eventData.UserId, eventData.Email, eventData.RegisteredAt);
        
        await Task.Delay(50, cancellationToken);
        
        _logger.LogInformation("✅ [Main-Singleton] Logging completed for {UserId}", eventData.UserId);
    }
}

// Main scoped handler
public class EmailNotificationHandler : IEventHandler<UserRegisteredEvent>
{
    private readonly ILogger<EmailNotificationHandler> _logger;

    public EmailNotificationHandler(ILogger<EmailNotificationHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(UserRegisteredEvent eventData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📧 [Main-Scoped] Sending welcome email to {Email}", eventData.Email);
        
        // Simulate email sending
        await Task.Delay(200, cancellationToken);
        
        _logger.LogInformation("✅ [Main-Scoped] Welcome email sent to {Email}", eventData.Email);
    }
}

// Another main scoped handler
public class UserProfileCreationHandler : IEventHandler<UserRegisteredEvent>
{
    private readonly ILogger<UserProfileCreationHandler> _logger;

    public UserProfileCreationHandler(ILogger<UserProfileCreationHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(UserRegisteredEvent eventData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("👤 [Main-Scoped] Creating user profile for {UserId}", eventData.UserId);
        
        // Simulate profile creation
        await Task.Delay(100, cancellationToken);
        
        _logger.LogInformation("✅ [Main-Scoped] User profile created for {UserId}", eventData.UserId);
    }
}

// Post-processing singleton handler
public class AnalyticsPostSingletonHandler : IEventPostSingletonHandler<UserRegisteredEvent>
{
    private readonly ILogger<AnalyticsPostSingletonHandler> _logger;

    public AnalyticsPostSingletonHandler(ILogger<AnalyticsPostSingletonHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(UserRegisteredEvent eventData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📊 [Post-Singleton] Recording analytics for user {UserId}", eventData.UserId);
        
        // Simulate analytics recording
        await Task.Delay(75, cancellationToken);
        
        _logger.LogInformation("✅ [Post-Singleton] Analytics recorded for {UserId}", eventData.UserId);
    }
}

// Post-processing scoped handler
public class CleanupPostHandler : IEventPostHandler<UserRegisteredEvent>
{
    private readonly ILogger<CleanupPostHandler> _logger;

    public CleanupPostHandler(ILogger<CleanupPostHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(UserRegisteredEvent eventData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🧹 [Post-Scoped] Cleaning up resources for user {UserId}", eventData.UserId);
        
        // Simulate cleanup
        await Task.Delay(50, cancellationToken);
        
        _logger.LogInformation("✅ [Post-Scoped] Cleanup completed for {UserId}", eventData.UserId);
    }
}

// Order handlers
public class OrderCreatedLoggerHandler : IEventSingletonHandler<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedLoggerHandler> _logger;

    public OrderCreatedLoggerHandler(ILogger<OrderCreatedLoggerHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(OrderCreatedEvent eventData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🛍️ [Main-Singleton] Order created: {OrderId} for customer {CustomerId} - Amount: ${Amount}", 
            eventData.OrderId, eventData.CustomerId, eventData.Amount);
        
        await Task.CompletedTask;
    }
}

public class InventoryUpdateHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly ILogger<InventoryUpdateHandler> _logger;

    public InventoryUpdateHandler(ILogger<InventoryUpdateHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(OrderCreatedEvent eventData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📦 [Main-Scoped] Updating inventory for order {OrderId}", eventData.OrderId);
        
        // Simulate inventory update
        await Task.Delay(150, cancellationToken);
        
        _logger.LogInformation("✅ [Main-Scoped] Inventory updated for order {OrderId}", eventData.OrderId);
    }
}
