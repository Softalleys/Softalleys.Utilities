using Softalleys.Utilities.Whatsapp.ObjectValues;
using Softalleys.Utilities.Whatsapp.Services;

namespace Softalleys.Utilities.Whatsapp;

/// <summary>
/// Represents a WhatsApp template message to be sent, including the recipient, template details, and components.
/// </summary>
/// <param name="To">The recipient's phone number in international format.</param>
/// <param name="Template">The WhatsApp template to use for the message.</param>
/// <param name="Components">An array of template components (e.g., header, body) with parameters.</param>
public record WhatsappTemplateMessage(
    string To,
    WhatsappTemplate Template,
    WhatsappTemplateComponent[] Components);