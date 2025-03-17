using Microsoft.AspNetCore.Http;

namespace Softalleys.Utilities.Extensions;

/// <summary>
/// Provides extension methods for converting files and arrays to and from byte arrays.
/// </summary>
public static class FileExtensions
{
    /// <summary>
    /// Converts the specified IFormFile instance to a byte array.
    /// </summary>
    /// <param name="file">The form file to convert.</param>
    /// <returns>A byte array containing the file's contents.</returns>
    public static byte[] ToByteArray(this IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        file.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Converts the specified float array (embeddings) to a byte array.
    /// </summary>
    /// <param name="embeddings">The float array to convert.</param>
    /// <returns>A byte array representing the converted floats.</returns>
    public static byte[] ToByteArray(this float[] embeddings)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);

        foreach (var embedding in embeddings)
        {
            writer.Write(embedding);
        }

        return memoryStream.ToArray();
    }

    /// <summary>
    /// Converts the specified byte array to a float array.
    /// </summary>
    /// <param name="embeddings">The byte array to convert.</param>
    /// <returns>A float array extracted from the byte array.</returns>
    public static float[] ToFloatArray(this byte[] embeddings)
    {
        using var memoryStream = new MemoryStream(embeddings);
        using var reader = new BinaryReader(memoryStream);

        var floatList = new List<float>();

        while (memoryStream.Position < memoryStream.Length)
        {
            floatList.Add(reader.ReadSingle());
        }

        return floatList.ToArray();
    }
}