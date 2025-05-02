using Google.Protobuf;

namespace Softalleys.Utilities.Protobuf;

/// <summary>
/// Provides extension methods for working with file-related operations.
/// </summary>
public static class FileExtensions
{
    /// <summary>
    /// Converts a byte array to a <see cref="ByteString"/>.
    /// </summary>
    /// <param name="byteArray">The byte array to convert.</param>
    /// <returns>A <see cref="ByteString"/> containing the data from the byte array.</returns>
    public static ByteString ToByteString(this byte[] byteArray)
    {
        return ByteString.CopyFrom(byteArray);
    }
}