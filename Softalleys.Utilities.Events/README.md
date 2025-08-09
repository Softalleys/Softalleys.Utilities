# Softalleys.Utilities.Events

A lightweight, flexible event-driven architecture library for .NET applications. This library provides a robust event handling system with proper dependency injection scope management, pre/post processing pipelines, and support for both scoped and singleton handler lifecycles.

## ✨ Features

- **🎯 Simple Event System**: Clean, intuitive interfaces for events and handlers
- **🔄 Flexible Handler Lifecycles**: Support for both scoped and singleton event handlers
- **⚡ Pre/Post Processing**: Built-in pipeline with pre-processing and post-processing phases
- **🏗️ Proper DI Integration**: Respects dependency injection scopes and lifecycles
- **📦 Auto-Discovery**: Automatic scanning and registration of event handlers from assemblies
- **🚀 High Performance**: Minimal overhead with concurrent handler execution
- **🛡️ Error Resilience**: Individual handler failures don't stop other handlers from executing
- **📝 Comprehensive Logging**: Built-in logging support for monitoring and debugging
- **🎨 Zero Dependencies**: Only depends on Microsoft.Extensions abstractions

## 🚀 Quick Start

### Installation

```bash
dotnet add package Softalleys.Utilities.Events
```

### 1. Define Your Events

```csharp
using Softalleys.Utilities.Events;

public class UserRegisteredEvent : IEvent
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
}

public class OrderCreatedEvent : IEvent
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CustomerId { get; set; } = string.Empty;
}
```

### 2. Create Event Handlers

```csharp
// Scoped handler - has access to current request scope (DbContext, etc.)
public class UserRegisteredEmailHandler(IEmailService emailService, IDbContext dbContext) : IEventHandler<UserRegisteredEvent>
{
    public async Task HandleAsync(UserRegisteredEvent eventData, CancellationToken cancellationToken = default)
    {
        await emailService.SendWelcomeEmailAsync(eventData.Email, cancellationToken);
        
        // Can access scoped services like DbContext
        var user = await dbContext.Users.FindAsync(eventData.UserId);
        if (user != null)
        {
            user.EmailSent = true;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

// Singleton handler - for stateless operations like logging
public class UserRegisteredLoggerHandler(ILogger<UserRegisteredLoggerHandler> logger) : IEventSingletonHandler<UserRegisteredEvent>
{
    public async Task HandleAsync(UserRegisteredEvent eventData, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("User {UserId} registered at {RegisteredAt}", 
            eventData.UserId, eventData.RegisteredAt);
        
        await Task.CompletedTask;
    }
}

// Pre-processing handler - runs before main handlers
public class UserValidationPreHandler(IUserValidationService validationService) : IEventPreHandler<UserRegisteredEvent>
{
    public async Task HandleAsync(UserRegisteredEvent eventData, CancellationToken cancellationToken = default)
    {
        await validationService.ValidateUserAsync(eventData.UserId, cancellationToken);
    }
}

// Post-processing handler - runs after main handlers
public class UserAnalyticsPostHandler(IAnalyticsService analyticsService) : IEventPostSingletonHandler<UserRegisteredEvent>
{
    public async Task HandleAsync(UserRegisteredEvent eventData, CancellationToken cancellationToken = default)
    {
        await analyticsService.TrackUserRegistrationAsync(eventData.UserId, cancellationToken);
    }
}
```

### 3. Register Services

```csharp
using Softalleys.Utilities.Events;

// In your Program.cs or Startup.cs
builder.Services.AddSoftalleysEvents(); // Scans current assembly

// Or specify multiple assemblies
builder.Services.AddSoftalleysEvents(
    typeof(UserRegisteredEvent).Assembly,
    typeof(OrderCreatedEvent).Assembly
);
```

### 4. Publish Events

```csharp
public class UserController(IEventBus eventBus) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request)
    {
        // Create user logic here...
        var userId = await CreateUserAsync(request);

        // Publish the event
        var userRegisteredEvent = new UserRegisteredEvent
        {
            UserId = userId,
            Email = request.Email,
            RegisteredAt = DateTime.UtcNow
        };

        await eventBus.PublishAsync(userRegisteredEvent);

        return Ok(new { UserId = userId });
    }
}
```

## 🎭 Handler Types and Execution Order

The library supports six different types of event handlers, executed in a specific order:

### Handler Types

| Handler Type | Lifetime | Purpose | When to Use |
|-------------|----------|---------|-------------|
| `IEventPreSingletonHandler<T>` | Singleton | Pre-processing | Stateless validation, logging setup |
| `IEventPreHandler<T>` | Scoped | Pre-processing | Database validation, scoped preparations |
| `IEventSingletonHandler<T>` | Singleton | Main processing | Stateless operations, caching, logging |
| `IEventHandler<T>` | Scoped | Main processing | Database operations, scoped business logic |
| `IEventPostSingletonHandler<T>` | Singleton | Post-processing | Analytics, cleanup, stateless notifications |
| `IEventPostHandler<T>` | Scoped | Post-processing | Final database updates, scoped cleanup |

### Execution Order

When you publish an event, handlers are executed in this order:

1. **Pre-processing Singleton Handlers** - `IEventPreSingletonHandler<T>`
2. **Pre-processing Scoped Handlers** - `IEventPreHandler<T>`
3. **Main Singleton Handlers** - `IEventSingletonHandler<T>`
4. **Main Scoped Handlers** - `IEventHandler<T>`
5. **Post-processing Singleton Handlers** - `IEventPostSingletonHandler<T>`
6. **Post-processing Scoped Handlers** - `IEventPostHandler<T>`

Within each phase, handlers execute concurrently for better performance.

## 🔧 Advanced Usage

### Multiple Handlers for Same Event

```csharp
// Multiple handlers can handle the same event
public class EmailNotificationHandler : IEventHandler<UserRegisteredEvent>
{
    public async Task HandleAsync(UserRegisteredEvent eventData, CancellationToken cancellationToken = default)
    {
        // Send email
    }
}

public class SmsNotificationHandler : IEventHandler<UserRegisteredEvent>  
{
    public async Task HandleAsync(UserRegisteredEvent eventData, CancellationToken cancellationToken = default)
    {
        // Send SMS
    }
}

public class SlackNotificationHandler : IEventSingletonHandler<UserRegisteredEvent>
{
    public async Task HandleAsync(UserRegisteredEvent eventData, CancellationToken cancellationToken = default)
    {
        // Send to Slack
    }
}
```

### Error Handling

```csharp
try
{
    await _eventBus.PublishAsync(myEvent);
}
catch (AggregateException ex)
{
    // Handle multiple handler failures
    foreach (var innerException in ex.InnerExceptions)
    {
        _logger.LogError(innerException, "Handler failed");
    }
}
```

### Complex Event Scenarios

```csharp
public class OrderProcessingEvent : IEvent
{
    public string OrderId { get; set; } = string.Empty;
    public List<string> ProductIds { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public string CustomerId { get; set; } = string.Empty;
}

// Pre-processing: Validate inventory
public class InventoryValidationPreHandler : IEventPreHandler<OrderProcessingEvent>
{
    private readonly IInventoryService _inventoryService;

    public InventoryValidationPreHandler(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public async Task HandleAsync(OrderProcessingEvent eventData, CancellationToken cancellationToken = default)
    {
        foreach (var productId in eventData.ProductIds)
        {
            var available = await _inventoryService.CheckAvailabilityAsync(productId, cancellationToken);
            if (!available)
            {
                throw new InvalidOperationException($"Product {productId} is out of stock");
            }
        }
    }
}

// Main processing: Create order
public class CreateOrderHandler : IEventHandler<OrderProcessingEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDbContext _dbContext;

    public CreateOrderHandler(IOrderRepository orderRepository, IDbContext dbContext)
    {
        _orderRepository = orderRepository;
        _dbContext = dbContext;
    }

    public async Task HandleAsync(OrderProcessingEvent eventData, CancellationToken cancellationToken = default)
    {
        var order = new Order
        {
            Id = eventData.OrderId,
            CustomerId = eventData.CustomerId,
            TotalAmount = eventData.TotalAmount,
            CreatedAt = DateTime.UtcNow
        };

        await _orderRepository.AddAsync(order, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

// Post-processing: Send notifications and update analytics
public class OrderAnalyticsPostHandler : IEventPostSingletonHandler<OrderProcessingEvent>
{
    private readonly IAnalyticsService _analyticsService;

    public OrderAnalyticsPostHandler(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    public async Task HandleAsync(OrderProcessingEvent eventData, CancellationToken cancellationToken = default)
    {
        await _analyticsService.TrackOrderAsync(eventData.OrderId, eventData.TotalAmount, cancellationToken);
    }
}
```

## 🏗️ Architecture Benefits

### Why Choose This Over MediatR or LiteBus?

| Feature | Softalleys.Events | MediatR | LiteBus |
|---------|------------------|---------|---------|
| **Cost** | ✅ Free | ❌ Requires License | ✅ Free |
| **DI Scope Support** | ✅ Full Support | ✅ Full Support | ❌ Transient Only |
| **Handler Lifecycles** | ✅ Scoped + Singleton | ✅ Configurable | ❌ Transient Only |
| **Pre/Post Processing** | ✅ Built-in | ✅ Via Behaviors | ❌ Manual |
| **Performance** | ✅ High | ✅ High | ✅ High |
| **Learning Curve** | ✅ Simple | ❌ Complex | ✅ Simple |

### Event-Driven Architecture Benefits

- **🔄 Loose Coupling**: Components don't need to know about each other directly
- **📈 Scalability**: Easy to add new handlers without modifying existing code
- **🧪 Testability**: Each handler can be tested independently
- **🔧 Maintainability**: Clear separation of concerns
- **🚀 Extensibility**: Simple to add new features via new handlers

## ⚡ Performance Considerations

- Handlers within the same phase execute concurrently for better throughput
- Singleton handlers are cached and reused, reducing allocation overhead
- Minimal reflection usage with caching for type discovery
- Efficient exception handling that doesn't stop other handlers

## 📋 Best Practices

### 1. Choose the Right Handler Type

```csharp
// ✅ Use scoped handlers for database operations
public class SaveUserHandler : IEventHandler<UserCreatedEvent>
{
    private readonly IDbContext _context;
    // ...
}

// ✅ Use singleton handlers for stateless operations
public class LogUserCreationHandler : IEventSingletonHandler<UserCreatedEvent>
{
    private readonly ILogger _logger;
    // ...
}
```

### 2. Keep Events Immutable

```csharp
// ✅ Good - immutable event
public class UserCreatedEvent : IEvent
{
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

// ❌ Avoid - mutable events can cause issues
public class UserCreatedEvent : IEvent
{
    public string UserId { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new(); // Handlers might modify this
}
```

### 3. Handle Failures Gracefully

```csharp
public class EmailHandler : IEventHandler<UserRegisteredEvent>
{
    public async Task HandleAsync(UserRegisteredEvent eventData, CancellationToken cancellationToken = default)
    {
        try
        {
            await _emailService.SendAsync(eventData.Email);
        }
        catch (EmailException ex)
        {
            // Log the error but don't throw - other handlers should still run
            _logger.LogError(ex, "Failed to send email to {Email}", eventData.Email);
            
            // Optionally, publish a compensation event
            await _eventBus.PublishAsync(new EmailFailedEvent 
            { 
                UserId = eventData.UserId, 
                Reason = ex.Message 
            });
        }
    }
}
```

### 4. Use Meaningful Event Names

```csharp
// ✅ Clear, domain-focused names
public class UserRegisteredEvent : IEvent { }
public class OrderShippedEvent : IEvent { }
public class PaymentProcessedEvent : IEvent { }

// ❌ Technical or vague names
public class UserEvent : IEvent { }
public class DataChangedEvent : IEvent { }
public class SomethingHappenedEvent : IEvent { }
```

## 🧪 Testing

### Unit Testing Handlers

```csharp
[Test]
public async Task UserRegisteredEmailHandler_ShouldSendWelcomeEmail()
{
    // Arrange
    var mockEmailService = new Mock<IEmailService>();
    var mockDbContext = new Mock<IDbContext>();
    var handler = new UserRegisteredEmailHandler(mockEmailService.Object, mockDbContext.Object);
    
    var eventData = new UserRegisteredEvent
    {
        UserId = "user123",
        Email = "test@example.com",
        RegisteredAt = DateTime.UtcNow
    };

    // Act
    await handler.HandleAsync(eventData);

    // Assert
    mockEmailService.Verify(x => x.SendWelcomeEmailAsync("test@example.com", It.IsAny<CancellationToken>()), Times.Once);
}
```

### Integration Testing

```csharp
[Test]
public async Task EventBus_ShouldExecuteAllHandlersInCorrectOrder()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddSoftalleysEvents(typeof(UserRegisteredEvent).Assembly);
    services.AddScoped<TestHandlerTracker>();
    // Add other required services...
    
    var serviceProvider = services.BuildServiceProvider();
    var eventBus = serviceProvider.GetRequiredService<IEventBus>();
    
    var eventData = new UserRegisteredEvent { UserId = "test", Email = "test@example.com" };

    // Act
    await eventBus.PublishAsync(eventData);

    // Assert
    var tracker = serviceProvider.GetRequiredService<TestHandlerTracker>();
    Assert.That(tracker.ExecutionOrder, Is.EqualTo(new[] { "Pre", "Main", "Post" }));
}
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

We welcome contributions! Please feel free to submit a Pull Request.

## 🏢 About Softalleys

This library is part of the Softalleys Utilities collection, designed to provide robust, enterprise-ready components for .NET applications while maintaining simplicity and performance.

---

**Happy Eventing! 🎉**
