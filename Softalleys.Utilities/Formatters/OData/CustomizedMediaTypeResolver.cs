using Microsoft.OData;
using Softalleys.Utilities.Formatters.OData.Csv;

namespace Softalleys.Utilities.Formatters.OData;

/// <summary>
/// Resolves customized media types to their corresponding formats.
/// </summary>
public class CustomizedMediaTypeResolver : ODataMediaTypeResolver
{
    /// <summary>
    /// A customized format shared for both YAML and CBOR media types.
    /// </summary>
    private static CustomizedFormat _customizedFormat = new CustomizedFormat();

    /// <summary>
    /// An array of media type formats including CSV, YAML, and CBOR.
    /// </summary>
    private readonly ODataMediaTypeFormat[] _mediaTypeFormats =
    {
        new ODataMediaTypeFormat(new ODataMediaType("text", "csv"), new CsvFormat()),
        new ODataMediaTypeFormat(new ODataMediaType("application", "yaml"), _customizedFormat),
        new ODataMediaTypeFormat(new ODataMediaType("application", "cbor"), _customizedFormat)
    };

    /// <summary>
    /// Gets the media type formats for the specified OData payload kind.
    /// For Resource and ResourceSet payload kinds, the custom media type formats are concatenated with the base formats.
    /// </summary>
    /// <param name="payloadKind">The kind of OData payload for which formats are requested.</param>
    /// <returns>An enumerable collection of <see cref="ODataMediaTypeFormat"/> objects.</returns>
    public override IEnumerable<ODataMediaTypeFormat> GetMediaTypeFormats(ODataPayloadKind payloadKind)
    {
        if (payloadKind == ODataPayloadKind.Resource || payloadKind == ODataPayloadKind.ResourceSet)
        {
            return _mediaTypeFormats.Concat(base.GetMediaTypeFormats(payloadKind));
        }

        return base.GetMediaTypeFormats(payloadKind);
    }
}