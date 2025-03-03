using System.Diagnostics.CodeAnalysis;

namespace Softalleys.Utilities.Extensions;

public static class ObjectExtensions
{
    public static T NotNull<T>([NotNull] this T? value, string valueName) where T : class
    {
        return value ?? throw new InvalidOperationException($"{valueName} is expected to be not null");
    }

    public static T NotNull<T>([NotNull] this T? value, string valueName) where T : struct
    {
        return value ?? throw new InvalidOperationException($"{valueName} is expected to be not null");
    }

    public static bool IsNull<T>(this T? value) where T : class
    {
        return value == null;
    }

    public static bool IsNull<T>(this T? value) where T : struct
    {
        return value == null;
    }
}