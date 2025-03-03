using Humanizer;

namespace Softalleys.Utilities.Extensions;

public static class EnumExtensions
{
    public static IEnumerable<string> ToSnakeCaseStrings<T>(this T enumValue) where T : Enum
    {
        return from Enum value in Enum.GetValues(enumValue.GetType())
            where enumValue.HasFlag(value)
            select value.ToString().Underscore();
    }
}