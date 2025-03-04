using System.Diagnostics.CodeAnalysis;

namespace Softalleys.Utilities.Extensions;

/// <summary>
/// Provides extension methods for object validation and null checks.
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Ensures that the specified value is not null.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="valueName">The name of the value parameter.</param>
    /// <returns>The original value if it is not null.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the value is null.</exception>
    public static T NotNull<T>([NotNull] this T? value, string valueName) where T : class
    {
        return value ?? throw new InvalidOperationException($"{valueName} is expected to be not null");
    }

    /// <summary>
    /// Ensures that the specified value is not null.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="valueName">The name of the value parameter.</param>
    /// <returns>The original value if it is not null.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the value is null.</exception>
    public static T NotNull<T>([NotNull] this T? value, string valueName) where T : struct
    {
        return value ?? throw new InvalidOperationException($"{valueName} is expected to be not null");
    }

    /// <summary>
    /// Checks if the specified value is null.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <returns>true if the value is null; otherwise, false.</returns>
    public static bool IsNull<T>(this T? value) where T : class
    {
        return value == null;
    }

    /// <summary>
    /// Checks if the specified value is null.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <returns>true if the value is null; otherwise, false.</returns>
    public static bool IsNull<T>(this T? value) where T : struct
    {
        return value == null;
    }
}