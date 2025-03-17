namespace Softalleys.Utilities.Interfaces;

/// <summary>
/// Defines a contract for entities to ensure they have a primary key property,
/// which is typically used as an identifier in Entity Framework.
/// </summary>
/// <typeparam name="T">
/// The type of the identifier.
/// </typeparam>
public interface IEntity<T>
{
    /// <summary>
    /// Gets the identifier for the entity.
    /// </summary>
    public T Id { get; init; }
}