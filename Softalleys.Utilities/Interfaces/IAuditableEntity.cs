namespace Softalleys.Utilities.Interfaces;

/// <summary>
/// Represents an auditable entity that captures creation and update timestamps.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// Gets the timestamp that indicates when the entity was created.
    /// </summary>
    DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets or sets the timestamp that indicates when the entity was last updated.
    /// </summary>
    DateTimeOffset UpdatedAt { get; set; }
}