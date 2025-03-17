using Microsoft.OData;

namespace Softalleys.Utilities.Formatters.OData.Csv;

/// <summary>
/// Resolves media types for OData data in CSV format.
/// </summary>
public class CsvMediaTypeResolver : ODataMediaTypeResolver
{
    /// <summary>
    /// Singleton instance of the <see cref="CsvMediaTypeResolver"/> class.
    /// </summary>
    private static readonly CsvMediaTypeResolver _instance = new CsvMediaTypeResolver();

    /// <summary>
    /// Array of supported media type formats for CSV.
    /// </summary>
    private readonly ODataMediaTypeFormat[] _mediaTypeFormats =
    {
        new ODataMediaTypeFormat(new ODataMediaType("text", "csv"), new CsvFormat()),
    };

    /// <summary>
    /// Gets the singleton instance of the <see cref="CsvMediaTypeResolver"/> class.
    /// </summary>
    public static CsvMediaTypeResolver Instance => _instance;

    /// <summary>
    /// Gets the media type formats supported for the specified OData payload kind.
    /// </summary>
    /// <param name="payloadKind">The kind of OData payload.</param>
    /// <returns>
    /// A collection of media type formats that are supported for the specified payload kind.
    /// Returns CSV formats along with base formats for Resource and ResourceSet payload kinds.
    /// </returns>
    public override IEnumerable<ODataMediaTypeFormat> GetMediaTypeFormats(ODataPayloadKind payloadKind)
    {
        if (payloadKind == ODataPayloadKind.Resource || payloadKind == ODataPayloadKind.ResourceSet)
        {
            return _mediaTypeFormats.Concat(base.GetMediaTypeFormats(payloadKind));
        }

        return base.GetMediaTypeFormats(payloadKind);
    }
}