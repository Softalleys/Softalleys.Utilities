namespace Softalleys.Utilities.Whatsapp.Options;

public record WhatsappBusinessOptions
{
    public string Token { get; set; } = string.Empty;

    public string DefaultTemplateName { get; set; } = string.Empty;

    public string DefaultTemplateLanguage { get; set; } = "en_US";

    public string BaseUrl { get; set; } = "https://graph.facebook.com";

    public string ApiVersion { get; set; } = "v22.0";

    public string WhatsAppBusinessAccountId { get; set; } = string.Empty;

    /// <summary>
    /// Default recipient phone number for text messages when no recipient is specified.
    /// Should be in international format (e.g., "1234567890").
    /// </summary>
    public string DefaultRecipient { get; set; } = string.Empty;
}