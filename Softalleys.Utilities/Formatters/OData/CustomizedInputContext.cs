using System.Text;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Softalleys.Utilities.Formatters.OData;

/// <summary>
/// Provides a customized OData input context for handling custom media types.
/// Based on https://github.com/xuzhg/MyAspNetCore/blob/master/src/ODataCustomizePayloadFormat/ODataCustomizePayloadFormat/
/// </summary>
public class CustomizedInputContext : ODataInputContext
{
    /// <summary>
    /// The media type of the input message.
    /// </summary>
    private ODataMediaType _mediaType;

    /// <summary>
    /// The message information containing stream and encoding details.
    /// </summary>
    private ODataMessageInfo _messageInfo;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomizedInputContext"/> class.
    /// </summary>
    /// <param name="format">The OData format to use.</param>
    /// <param name="settings">The message reader settings.</param>
    /// <param name="messageInfo">Information about the input message including stream and media type.</param>
    public CustomizedInputContext(ODataFormat format, ODataMessageReaderSettings settings, ODataMessageInfo messageInfo)
        : base(format, messageInfo, settings)
    {
        MessageStream = messageInfo.MessageStream;
        _mediaType = messageInfo.MediaType;
        _messageInfo = messageInfo;
    }

    /// <summary>
    /// Gets the input message stream.
    /// </summary>
    public Stream MessageStream { get; private set; }

    /// <summary>
    /// Creates an OData reader for reading a resource set asynchronously.
    /// </summary>
    /// <param name="entitySet">The entity set metadata.</param>
    /// <param name="resourceType">The type of resources in the set.</param>
    /// <returns>A task that represents the asynchronous operation, containing the OData reader.</returns>
    public override Task<ODataReader> CreateResourceSetReaderAsync(IEdmEntitySetBase entitySet, IEdmStructuredType resourceType)
    {
        ODataReader reader = CreateReader(resourceType);
        return Task.FromResult<ODataReader>(reader);
    }

    /// <summary>
    /// Creates an OData reader for reading a single resource asynchronously.
    /// </summary>
    /// <param name="navigationSource">The navigation source metadata.</param>
    /// <param name="resourceType">The type of the resource.</param>
    /// <returns>A task that represents the asynchronous operation, containing the OData reader.</returns>
    public override Task<ODataReader> CreateResourceReaderAsync(IEdmNavigationSource navigationSource, IEdmStructuredType resourceType)
    {
        ODataReader reader = CreateReader(resourceType);
        return Task.FromResult<ODataReader>(reader);
    }

    /// <summary>
    /// Creates an appropriate OData reader based on the media type.
    /// </summary>
    /// <param name="resourceType">The type of resource to read.</param>
    /// <returns>An OData reader instance for the specified media type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the media type is not supported.</exception>
    private ODataReader CreateReader(IEdmStructuredType resourceType)
    {
        TextReader textReader = CreateReader(_messageInfo.MessageStream, _messageInfo.Encoding);

        // if (_mediaType.Type == "text" && _mediaType.SubType == "csv")
        // {
        //     return new CsvODataReader(textReader, resourceType);
        // }
        //
        // if (_mediaType.Type == "application" && _mediaType.SubType == "yaml")
        // {
        //     return new YamlODataReader(textReader, resourceType);
        // }

        throw new InvalidOperationException($"Not valid '{_mediaType.Type}/{_mediaType.SubType}' for this output context.");
    }

    /// <summary>
    /// Creates a TextReader for the input stream with the specified encoding.
    /// </summary>
    /// <param name="messageStream">The input stream to read from.</param>
    /// <param name="encoding">The encoding to use when reading the stream.</param>
    /// <returns>A TextReader instance configured with the specified stream and encoding.</returns>
    private static TextReader CreateReader(Stream messageStream, Encoding encoding)
    {
        return new StreamReader(messageStream, encoding);
    }
}