using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Softalleys.Utilities.Extensions;

/// <summary>
///     The class provides extension methods for enhancing the functionality and ease of use of strings.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    ///     Inserts a specified value into the source string after a specified fragment.
    /// </summary>
    /// <param name="source">The source string where the value will be inserted.</param>
    /// <param name="fragment">The fragment after which the value will be inserted.</param>
    /// <param name="value">The value to insert into the source string.</param>
    /// <returns>A new string with the value inserted.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the fragment is not found in the source string.</exception>
    public static string InsertAfter(this string source, string fragment, string value)
    {
        var i = source.IndexOf(fragment, StringComparison.Ordinal);
        if (i < 0) throw new InvalidOperationException($"Can't find {fragment}");

        return source.Insert(i + fragment.Length, value);
    }

    /// <summary>
    ///     Converts the specified string to a GUID.
    /// </summary>
    /// <param name="value">The string to convert to a GUID.</param>
    /// <returns>The GUID representation of the string.</returns>
    /// <exception cref="FormatException">Thrown when the string is not a valid GUID.</exception>
    public static Guid ToGuid(this string value)
    {
        if (Guid.TryParse(value, out var result)) return result;

        throw new FormatException($"The value '{value}' is not a valid GUID.");
    }

    /// <summary>
    ///     Determines whether the specified string is a valid GUID.
    /// </summary>
    /// <param name="value">The string to test.</param>
    /// <returns>true if the string is a valid GUID; otherwise, false.</returns>
    public static bool IsGuid(this string value)
    {
        return Guid.TryParse(value, out _);
    }

    /// <summary>
    ///     Converts the specified string to a nullable GUID.
    /// </summary>
    /// <param name="value">The string to convert to a nullable GUID.</param>
    /// <returns>The nullable GUID representation of the string, or null if the string is not a valid GUID.</returns>
    public static Guid? ToGuidOrNull(this string value)
    {
        if (Guid.TryParse(value, out var result)) return result;

        return null;
    }

    /// <summary>
    ///     Determines whether the specified string is neither null nor empty.
    /// </summary>
    /// <param name="value">The string to test.</param>
    /// <returns>true if the value parameter is not null or an empty string (""); otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [DebuggerStepThrough]
    public static bool HasValue([NotNullWhen(true)] this string? value)
    {
        return !string.IsNullOrEmpty(value);
    }

    /// <summary>
    ///     Trims the specified suffix from the end of the string, if it exists.
    /// </summary>
    /// <param name="source">The source string to trim.</param>
    /// <param name="suffix">The suffix to remove if it exists at the end of the source string.</param>
    /// <returns>The string without the specified suffix.</returns>
    public static string TrimSuffixIfExists(this string source, string suffix)
    {
        return !string.IsNullOrEmpty(suffix) && source.EndsWith(suffix) ? source[..^suffix.Length] : source;
    }

    /// <summary>
    ///     Ensures that a string is neither null nor empty, throwing an exception if it is.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="valueName">The name of the string variable, used in the exception message.</param>
    /// <returns>The original string if it is neither null nor empty.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the string is null or empty.</exception>
    [DebuggerStepThrough]
    public static string NotNullOrEmpty([NotNull] this string? value, string valueName)
    {
        return !string.IsNullOrEmpty(value)
            ? value
            : throw new InvalidOperationException($"{valueName} is expected to be not null or empty");
    }
}