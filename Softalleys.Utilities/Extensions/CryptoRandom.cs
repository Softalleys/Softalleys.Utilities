using System.Security.Cryptography;

namespace Softalleys.Utilities.Extensions;

/// <summary>
/// Provides methods for generating cryptographically strong random values.
/// </summary>
public static class CryptoRandom
{
    /// <summary>
    /// Generates a specified number of random bytes.
    /// </summary>
    /// <param name="count">The number of random bytes to generate. Default is 32.</param>
    /// <returns>An array of bytes filled with cryptographically strong random values.</returns>
    public static byte[] GetRandomBytes(int count = 32)
    {
        var buffer = new byte[count];
        using var random = RandomNumberGenerator.Create();
        random.GetBytes(buffer);
        return buffer;
    }

    /// <summary>
    /// Generates a random string of the specified length using cryptographically strong random values.
    /// </summary>
    /// <param name="length">The length of the random string to generate. Default is 32.</param>
    /// <param name="withSpecialChars">Indicates whether to include special characters in the random string. Default is false.</param>
    /// <returns>A random string of the specified length.</returns>
    public static string GetRandomString(int length = 32, bool withSpecialChars = false)
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        if (withSpecialChars) chars += "!@#$%^&*()_+-=[]{}|;:,.<>?";

        var random = GetRandomBytes(length);
        var result = new char[length];
        for (var i = 0; i < length; i++) result[i] = chars[random[i] % chars.Length];

        return new string(result);
    }
}