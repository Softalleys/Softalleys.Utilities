using Softalleys.Utilities.Whatsapp.Results;

namespace Softalleys.Utilities.Whatsapp;

public interface IWhatsappMessageService
{
    /// <summary>
    /// Sends a WhatsApp message.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<WhatsappMessageResult> SendMessageAsync(string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a WhatsApp template message.
    /// </summary>
    /// <param name="message">The WhatsApp template message to send.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the result of the message send.</returns>
    Task<WhatsappMessageResult> SendMessageAsync(WhatsappTemplateMessage message,
        CancellationToken cancellationToken = default);
}