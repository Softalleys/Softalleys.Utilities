namespace Softalleys.Utilities.Extensions;

/// <summary>
/// Provides methods for converting data to and from hexadecimal strings.
/// </summary>
public static class HexadecimalConverter
{
    /// <summary>
    /// Converts a byte array to a hexadecimal string.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <returns>A hexadecimal string representation of the byte array.</returns>
    public static string ToHexadecimalString(this byte[] bytes)
    {
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// Converts a hexadecimal string to a byte array.
    /// </summary>
    /// <param name="hex">The hexadecimal string to convert.</param>
    /// <returns>A byte array representation of the hexadecimal string.</returns>
    public static byte[] FromHexadecimalString(this string hex)
    {
        return Convert.FromHexString(hex);
    }

    /// <summary>
    /// Converts a byte array to a hexadecimal string with an option for uppercase.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="upperCase">If true, the hexadecimal string will be in uppercase.</param>
    /// <returns>A hexadecimal string representation of the byte array.</returns>
    public static string ToHexadecimalString(this byte[] bytes, bool upperCase)
    {
        return Convert.ToHexString(bytes).ToUpperInvariant();
    }

    /// <summary>
    /// Converts an integer to a hexadecimal string.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    /// <returns>A hexadecimal string representation of the integer.</returns>
    public static string ToHexadecimalString(int value)
    {
        return value.ToString("X");
    }

    /// <summary>
    /// Converts a long integer to a hexadecimal string.
    /// </summary>
    /// <param name="value">The long integer value to convert.</param>
    /// <returns>A hexadecimal string representation of the long integer.</returns>
    public static string ToHexadecimalString(long value)
    {
        return value.ToString("X");
    }

    /// <summary>
    /// Converts a short integer to a hexadecimal string.
    /// </summary>
    /// <param name="value">The short integer value to convert.</param>
    /// <returns>A hexadecimal string representation of the short integer.</returns>
    public static string ToHexadecimalString(short value)
    {
        return value.ToString("X");
    }
}