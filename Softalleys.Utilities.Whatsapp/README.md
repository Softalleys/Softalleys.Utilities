# WhatsApp Business Message Service

The `Softalleys.Utilities.Whatsapp` library provides a simple and robust way to send WhatsApp messages using the WhatsApp Business API.

## Installation

```bash
dotnet add package Softalleys.Utilities.Whatsapp
```

## Configuration

### Using Dependency Injection with Action Delegate

```csharp
using Microsoft.Extensions.DependencyInjection;
using Softalleys.Utilities.Whatsapp;

var services = new ServiceCollection();

services.AddWhatsappMessageService(options =>
{
    options.Token = "your-whatsapp-business-api-token";
    options.WhatsAppBusinessAccountId = "your-business-account-id";
    options.DefaultRecipient = "1234567890"; // For text messages
    options.BaseUrl = "https://graph.facebook.com"; // Default
    options.ApiVersion = "v22.0"; // Default
    options.DefaultTemplateLanguage = "en_US"; // Default
});
```

### Using Configuration from appsettings.json

```json
{
  "WhatsappBusiness": {
    "Token": "your-whatsapp-business-api-token",
    "WhatsAppBusinessAccountId": "your-business-account-id",
    "DefaultRecipient": "1234567890",
    "BaseUrl": "https://graph.facebook.com",
    "ApiVersion": "v22.0",
    "DefaultTemplateLanguage": "en_US"
  }
}
```

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Softalleys.Utilities.Whatsapp;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var services = new ServiceCollection();
services.AddWhatsappMessageService(configuration);
```

### Using Custom Configuration Section

```csharp
services.AddWhatsappMessageService(configuration, "MyWhatsAppConfig");
// or
services.AddWhatsappMessageService(configuration.GetSection("MyWhatsAppConfig"));
```

## Usage

### Sending Text Messages

```csharp
using Softalleys.Utilities.Whatsapp;

public class MessageService
{
    private readonly IWhatsappMessageService _whatsappService;

    public MessageService(IWhatsappMessageService whatsappService)
    {
        _whatsappService = whatsappService;
    }

    public async Task SendSimpleMessage()
    {
        try
        {
            var result = await _whatsappService.SendMessageAsync(
                "Hello! This is a test message from our application.",
                CancellationToken.None);
            
            Console.WriteLine($"Message sent successfully! ID: {result.Id}, Status: {result.Status}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send message: {ex.Message}");
        }
    }
}
```

### Sending Template Messages

```csharp
using Softalleys.Utilities.Whatsapp;
using Softalleys.Utilities.Whatsapp.ObjectValues;

public async Task SendTemplateMessage()
{
    var templateMessage = new WhatsappTemplateMessage(
        To: "1234567890",
        Template: new WhatsappTemplate("hello_world", "en_US"),
        Components: new[]
        {
            new WhatsappTemplateComponent("body", new[]
            {
                new WhatsappTemplateParameterText("John Doe"),
                new WhatsappTemplateParameterText("Welcome to our service!")
            })
        });

    try
    {
        var result = await _whatsappService.SendMessageAsync(templateMessage);
        Console.WriteLine($"Template message sent! ID: {result.Id}, Status: {result.Status}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to send template message: {ex.Message}");
    }
}
```

### Advanced Template with Media

```csharp
public async Task SendTemplateWithMedia()
{
    var templateMessage = new WhatsappTemplateMessage(
        To: "1234567890",
        Template: new WhatsappTemplate("media_template", "en_US"),
        Components: new[]
        {
            new WhatsappTemplateComponent("header", new[]
            {
                new WhatsappTemplateParameterMedia(
                    "https://example.com/image.jpg",
                    "Product Image")
            }),
            new WhatsappTemplateComponent("body", new[]
            {
                new WhatsappTemplateParameterText("Customer Name"),
                new WhatsappTemplateParameterText("Product Details")
            }),
            new WhatsappTemplateComponent("button", new[]
            {
                new WhatsappTemplateParameterButton("View Product", "product_123")
            })
        });

    var result = await _whatsappService.SendMessageAsync(templateMessage);
}
```

## Error Handling

The service provides comprehensive error handling with specific exception types:

```csharp
try
{
    var result = await _whatsappService.SendMessageAsync("Hello World!");
}
catch (ArgumentException ex)
{
    // Invalid input parameters (empty message, invalid phone number, etc.)
    Console.WriteLine($"Invalid input: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    // Configuration issues or API errors
    Console.WriteLine($"Operation error: {ex.Message}");
}
catch (OperationCanceledException ex)
{
    // Request timeout or cancellation
    Console.WriteLine($"Operation cancelled: {ex.Message}");
}
catch (Exception ex)
{
    // Unexpected errors
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

## Configuration Options

| Property | Description | Required | Default |
|----------|-------------|----------|---------|
| `Token` | WhatsApp Business API access token | Yes | - |
| `WhatsAppBusinessAccountId` | Your WhatsApp Business Account ID | Yes | - |
| `DefaultRecipient` | Default phone number for text messages | Yes* | - |
| `BaseUrl` | WhatsApp API base URL | No | `https://graph.facebook.com` |
| `ApiVersion` | API version to use | No | `v22.0` |
| `DefaultTemplateLanguage` | Default language for templates | No | `en_US` |
| `DefaultTemplateName` | Default template name | No | - |

*Required for text messages using `SendMessageAsync(string message)`

## Phone Number Format

Phone numbers should be provided in international format without the `+` sign:
- ✅ Correct: `1234567890`
- ✅ Correct: `+1234567890` (optional + prefix)
- ❌ Incorrect: `(123) 456-7890`
- ❌ Incorrect: `123-456-7890`

## WhatsApp Message Status

The service returns a `WhatsappMessageResult` with the following possible statuses:

- `Accepted` - Message was accepted by WhatsApp
- `Queued` - Message is queued for delivery
- `Sending` - Message is being sent
- `Failed` - Message failed to send
- `AuthError` - Authentication error (invalid token)

## Thread Safety

The `WhatsappBusinessMessageService` is thread-safe and can be registered as a singleton in the DI container.

## License

This library is part of the Softalleys.Utilities collection and follows the same licensing terms.