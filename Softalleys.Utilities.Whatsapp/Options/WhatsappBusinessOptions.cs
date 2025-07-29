namespace Softalleys.Utilities.Whatsapp.Options;

public record WhatsappBusinessOptions
{
    public string Token { get; init; } = string.Empty;

    public string DefaultTemplateName { get; init; } = string.Empty;

    public string DefaultTemplateLanguage { get; init; } = "en_US";

    public string BaseUrl { get; init; } = "https://graph.facebook.com";

    public string ApiVersion { get; init; } = "v22.0";

    public string WhatsAppBusinessAccountId { get; init; } = string.Empty;
}