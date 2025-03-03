namespace Softalleys.Utilities.Extensions;

public static class HexadecimalConverter
{
    public static string ToHexadecimalString(this byte[] bytes)
    {
        return Convert.ToHexString(bytes);
    }

    public static byte[] FromHexadecimalString(this string hex)
    {
        return Convert.FromHexString(hex);
    }

    public static string ToHexadecimalString(this byte[] bytes, bool upperCase)
    {
        return Convert.ToHexString(bytes).ToUpperInvariant();
    }

    public static string ToHexadecimalString(int value)
    {
        return value.ToString("X");
    }

    public static string ToHexadecimalString(long value)
    {
        return value.ToString("X");
    }

    public static string ToHexadecimalString(short value)
    {
        return value.ToString("X");
    }
}