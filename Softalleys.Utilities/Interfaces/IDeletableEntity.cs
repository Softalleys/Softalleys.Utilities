namespace Softalleys.Utilities.Interfaces;

/// <summary>
/// Provides a contract for soft deletion of an entity.
/// </summary>
public interface IDeletableEntity
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity is deleted.
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the entity was marked as deleted.
    /// </summary>
    DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who deleted the entity.
    /// </summary>
    Guid? DeletedBy { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the entity was forcibly deleted.
    /// </summary>
    DateTimeOffset? ForcedDeletedAt { get; set; }

    /// <summary>
    /// Marks the entity as deleted.
    /// </summary>
    /// <param name="deletedBy">The identifier of the user who deleted the entity. Optional.</param>
    void SoftDelete(Guid? deletedBy = null);

    /// <summary>
    /// Restores the entity from a deleted state.
    /// </summary>
    void Restore();
}