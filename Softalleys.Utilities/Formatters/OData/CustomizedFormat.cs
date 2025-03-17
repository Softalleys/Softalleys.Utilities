using Microsoft.OData;

namespace Softalleys.Utilities.Formatters.OData;

/// <summary>
/// Provides a customized OData format implementation for handling custom media types.
/// Based on https://github.com/xuzhg/MyAspNetCore/blob/master/src/ODataCustomizePayloadFormat/ODataCustomizePayloadFormat/
/// </summary>
public class CustomizedFormat : ODataFormat
{
    /// <summary>
    /// Creates an asynchronous output context for writing OData messages in the custom format.
    /// </summary>
    /// <param name="messageInfo">Information about the OData message to be written.</param>
    /// <param name="messageWriterSettings">Settings for the message writer.</param>
    /// <returns>A task that represents the asynchronous operation, containing the output context.</returns>
    public override Task<ODataOutputContext> CreateOutputContextAsync(
        ODataMessageInfo messageInfo, ODataMessageWriterSettings messageWriterSettings)
    {
        return Task.FromResult<ODataOutputContext>(
            new CustomizedOutputContext(this, messageWriterSettings, messageInfo));
    }

    /// <summary>
    /// Creates an asynchronous input context for reading OData messages in the custom format.
    /// </summary>
    /// <param name="messageInfo">Information about the OData message to be read.</param>
    /// <param name="messageReaderSettings">Settings for the message reader.</param>
    /// <returns>A task that represents the asynchronous operation, containing the input context.</returns>
    public override Task<ODataInputContext> CreateInputContextAsync(
        ODataMessageInfo messageInfo, ODataMessageReaderSettings messageReaderSettings)
    {
        return Task.FromResult<ODataInputContext>(
            new CustomizedInputContext(this, messageReaderSettings, messageInfo));
    }

    #region Synchronization not used

    /// <summary>
    /// This synchronous method is not implemented.
    /// </summary>
    /// <exception cref="NotImplementedException">Always thrown as this method is not supported.</exception>
    public override ODataInputContext CreateInputContext(
        ODataMessageInfo messageInfo, ODataMessageReaderSettings messageReaderSettings)
        => throw new NotImplementedException();

    /// <summary>
    /// This synchronous method is not implemented.
    /// </summary>
    /// <exception cref="NotImplementedException">Always thrown as this method is not supported.</exception>
    public override ODataOutputContext CreateOutputContext(
        ODataMessageInfo messageInfo, ODataMessageWriterSettings messageWriterSettings)
        => throw new NotImplementedException();

    /// <summary>
    /// This synchronous method is not implemented.
    /// </summary>
    /// <exception cref="NotImplementedException">Always thrown as this method is not supported.</exception>
    public override IEnumerable<ODataPayloadKind> DetectPayloadKind(
        ODataMessageInfo messageInfo, ODataMessageReaderSettings settings)
        => throw new NotImplementedException();

    /// <summary>
    /// This asynchronous method is not implemented.
    /// </summary>
    /// <exception cref="NotImplementedException">Always thrown as this method is not supported.</exception>
    public override Task<IEnumerable<ODataPayloadKind>> DetectPayloadKindAsync(
        ODataMessageInfo messageInfo, ODataMessageReaderSettings settings)
        => throw new NotImplementedException();

    #endregion
}