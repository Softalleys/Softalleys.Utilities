namespace Softalleys.Utilities.Commands;

/// <summary>
/// Default command handler that orchestrates validation and processing using provided validators and processors.
/// Register this for a given command/result pair to get the standard pipeline behavior.
/// </summary>
public class DefaultCommandHandler<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly IReadOnlyList<ICommandValidator<TCommand, TResult>> _validators;
    private readonly IReadOnlyList<ICommandProcessor<TCommand, TResult>> _processors;
    private readonly IReadOnlyList<ICommandPostAction<TCommand, TResult>> _postActions;

    // Primary constructor for multiple validators and processors
    public DefaultCommandHandler(
        IEnumerable<ICommandValidator<TCommand, TResult>>? validators,
        IEnumerable<ICommandProcessor<TCommand, TResult>> processors,
        IEnumerable<ICommandPostAction<TCommand, TResult>>? postActions = null)
    {
        _validators = (validators ?? Enumerable.Empty<ICommandValidator<TCommand, TResult>>()).ToList();
        _processors = (processors ?? Enumerable.Empty<ICommandProcessor<TCommand, TResult>>()).ToList();
        _postActions = (postActions ?? Enumerable.Empty<ICommandPostAction<TCommand, TResult>>()).ToList();

        if (_processors.Count == 0)
            throw new ArgumentException("At least one processor must be provided.", nameof(processors));
    }

    // Backward compatibility constructor for single validator and processor
    public DefaultCommandHandler(
        ICommandValidator<TCommand, TResult>? validator,
        ICommandProcessor<TCommand, TResult> processor,
        IEnumerable<ICommandPostAction<TCommand, TResult>>? postActions = null)
        : this(
            validator != null ? new[] { validator } : null,
            new[] { processor ?? throw new ArgumentNullException(nameof(processor)) },
            postActions)
    {
    }

    public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        // Run all validators in sequence until one fails
        foreach (var validator in _validators)
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
            // Domain decides whether this result means "continue" or return.
            // Convention: if validator returns a type equal to TResult but not a marker that indicates Valid, the domain can choose.
            // Common pattern is to return a specific Valid result. We detect it via interface or named type optionally.
            if (!ShouldContinueAfterValidation(validationResult))
            {
                return validationResult;
            }
        }

        // Execute all processors in sequence - each processor gets the original command
        // The last processor's result is the final result
        TResult result = default(TResult)!;
        foreach (var processor in _processors)
        {
            result = await processor.ProcessAsync(command, cancellationToken).ConfigureAwait(false);
        }

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
