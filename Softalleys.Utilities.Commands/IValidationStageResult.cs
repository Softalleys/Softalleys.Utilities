namespace Softalleys.Utilities.Commands;

/// <summary>
/// Optional contract used by validation result types to indicate whether the pipeline should continue to processing.
/// </summary>
public interface IValidationStageResult
{
    bool Continue { get; }
}
