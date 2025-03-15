namespace Softalleys.Utilities.Extensions;

/// <summary>
///     Provides extension methods for converting ULID strings to <see cref="DateTimeOffset"/> values 
///     and for creating ULIDs from <see cref="DateTimeOffset"/> values.
/// </summary>
public static class UlidExtensions
{
    /// <summary>
    ///     Converts a ULID string to a <see cref="DateTimeOffset"/> representing the time portion of the ULID.
    /// </summary>
    /// <param name="ulid">
    ///     The ULID string to convert.
    /// </param>
    /// <returns>
    ///     A <see cref="DateTimeOffset"/> corresponding to the ULID's time, or <c>null</c> if parsing fails.
    /// </returns>
    public static DateTimeOffset? FromUlid(this string ulid)
    {
        if (Ulid.TryParse(ulid, out var parsedUlid))
        {
            return parsedUlid.Time;
        }
        
        return null;
    }
    
    /// <summary>
    ///     Converts a <see cref="DateTimeOffset"/> to a ULID string.
    /// </summary>
    /// <param name="dateTimeOffset">
    ///     The <see cref="DateTimeOffset"/> to convert.
    /// </param>
    /// <returns>
    ///     A ULID string generated from the specified <see cref="DateTimeOffset"/>.
    /// </returns>
    public static string ToUlidString(this DateTimeOffset dateTimeOffset)
    {
        var ulid = Ulid.NewUlid(dateTimeOffset);
        return ulid.ToString();
    }

    /// <summary>
    ///     Converts a <see cref="DateTimeOffset"/> to a <see cref="Ulid"/> instance.
    /// </summary>
    /// <param name="dateTimeOffset">
    ///     The <see cref="DateTimeOffset"/> from which to generate the ULID.
    /// </param>
    /// <returns>
    ///     A <see cref="Ulid"/> generated from the specified <see cref="DateTimeOffset"/>.
    /// </returns>
    public static Ulid ToUlid(this DateTimeOffset dateTimeOffset)
    {
        return Ulid.NewUlid(dateTimeOffset);
    }
}