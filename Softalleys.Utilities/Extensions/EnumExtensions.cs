using Humanizer;

namespace Softalleys.Utilities.Extensions;

/// <summary>
/// Provides extension methods for enumerations.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Converts the flags of an enumeration value to a collection of snake_case strings.
    /// </summary>
    /// <typeparam name="T">The type of the enumeration.</typeparam>
    /// <param name="enumValue">The enumeration value.</param>
    /// <returns>An enumerable collection of snake_case strings representing the flags of the enumeration value.</returns>
    public static IEnumerable<string> ToSnakeCaseStrings<T>(this T enumValue) where T : Enum
    {
        return from Enum value in Enum.GetValues(enumValue.GetType())
            where enumValue.HasFlag(value)
            select value.ToString().Underscore();
    }
}