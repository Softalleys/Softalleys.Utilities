namespace Softalleys.Utilities.Commands;

/// <summary>
/// Default command handler that orchestrates validation and processing using provided validator and processor.
/// Register this for a given command/result pair to get the standard pipeline behavior.
/// </summary>
public class DefaultCommandHandler<TCommand, TResult>(
    ICommandValidator<TCommand, TResult>? validator,
    ICommandProcessor<TCommand, TResult> processor,
    IEnumerable<ICommandPostAction<TCommand, TResult>>? postActions = null)
    : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly ICommandValidator<TCommand, TResult>? _validator = validator;
    private readonly ICommandProcessor<TCommand, TResult> _processor = processor ?? throw new ArgumentNullException(nameof(processor));
    private readonly IReadOnlyList<ICommandPostAction<TCommand, TResult>> _postActions = (postActions ?? Enumerable.Empty<ICommandPostAction<TCommand, TResult>>()).ToList();

    public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        if (_validator is not null)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
            // Domain decides whether this result means "continue" or return.
            // Convention: if validator returns a type equal to TResult but not a marker that indicates Valid, the domain can choose.
            // Common pattern is to return a specific Valid result. We detect it via interface or named type optionally.
            if (!ShouldContinueAfterValidation(validationResult))
            {
                return validationResult;
            }
        }

        var result = await _processor.ProcessAsync(command, cancellationToken).ConfigureAwait(false);

        // Execute post actions sequentially to avoid concurrency issues with scoped resources (e.g., DbContext)
        if (_postActions.Count > 0)
        {
            foreach (var action in _postActions)
            {
                await action.ExecuteAsync(command, result, cancellationToken).ConfigureAwait(false);
            }
        }

        return result;
    }

    private static bool ShouldContinueAfterValidation(TResult validationResult)
    {
        // Heuristic: if TResult implements IValidationStageResult, use its Continue flag; otherwise assume continue only if type name ends with "Valid".
        if (validationResult is IValidationStageResult vsr)
        {
            return vsr.Continue;
        }

        var typeName = validationResult?.GetType().Name ?? string.Empty;
        return typeName.EndsWith("Valid", StringComparison.Ordinal);
    }
}
