using Softalleys.Utilities.Events;

namespace Softalleys.Utilities.Events.Example;

// Example events
public class UserRegisteredEvent : IEvent
{
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime RegisteredAt { get; init; }
}

public class OrderCreatedEvent : IEvent
{
    public string OrderId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string CustomerId { get; init; } = string.Empty;
}
