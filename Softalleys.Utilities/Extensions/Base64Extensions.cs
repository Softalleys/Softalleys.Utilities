namespace Softalleys.Utilities.Extensions;

/// <summary>
///     Provides extension methods for converting byte arrays to and from Base64 strings.
/// </summary>
public static class Base64Extensions
{
    /// <summary>
    ///     Converts a byte array to a Base64 encoded string.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <returns>A Base64 encoded string representation of the byte array.</returns>
    public static string ToBase64(this byte[] bytes)
    {
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    ///     Converts a Base64 encoded string to a byte array.
    /// </summary>
    /// <param name="base64">The Base64 encoded string to convert.</param>
    /// <returns>A byte array representation of the Base64 encoded string.</returns>
    public static byte[] FromBase64(this string base64)
    {
        return Convert.FromBase64String(base64);
    }

    /// <summary>
    ///     Converts a byte array to a Base64 encoded image string with a specified format.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="format">The format of the image (default is "image/jpeg").</param>
    /// <returns>A Base64 encoded image string with the specified format.</returns>
    public static string ToBase64Image(this byte[] bytes, string format = "image/jpeg")
    {
        return $"data:{format};base64,{Convert.ToBase64String(bytes)}";
    }
}