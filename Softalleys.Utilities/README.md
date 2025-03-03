# Softalleys.Utilities

A comprehensive collection of utility classes, extension methods, and validation attributes for .NET applications.

## Installation

```shell
dotnet add package Softalleys.Utilities
```

## Core Utilities

### ParametersBuilder

A builder for constructing and manipulating query strings or URI fragment parts.

```csharp
// Create a new parameters builder
var builder = new ParametersBuilder();

// Add parameters
builder["key"] = "value";

// Convert to string
string queryString = builder.ToString(); // "key=value"
```

### UriBuilderWithQuery

A wrapper around `System.UriBuilder` with enhanced functionality for URI manipulation, specifically for handling query strings and fragments.

```csharp
// Create a builder from a URI string
var builder = new UriBuilderWithQuery("https://example.com");

// Add query parameters
builder.Query["key"] = "value";

// Add fragment parameters
builder.Fragment["section"] = "details";

// Get the resulting URI
Uri result = builder.Uri; // https://example.com?key=value#section=details
```

## Extensions

### ObjectExtensions

Methods for working with object references.

```csharp
// Check if an object is null
if (myObject.IsNull())

// Throw exception if null
var notNullObject = myObject.NotNull("myObject");
```

### StringExtensions

Methods for enhancing string functionality.

```csharp
// Insert text after a specific fragment
string result = "Hello world".InsertAfter("Hello ", "beautiful ");

// Convert string to GUID
Guid guid = "c2d06c59-9b9e-4e2a-b9d6-d9822937c8c4".ToGuid();

// Check if string is a valid GUID
bool isGuid = "c2d06c59-9b9e-4e2a-b9d6-d9822937c8c4".IsGuid();

// Check if string has a value (not null or empty)
if (myString.HasValue())

// Ensure a string is not null or empty
string value = myString.NotNullOrEmpty("myString");
```

### Base32

Provides methods for encoding and decoding data using Base32 and Base32Hex encoding schemes.

```csharp
// Encode data to Base32
string encoded = Base32.Encode(byteArray);

// Encode data to Base32Hex
string hexEncoded = Base32.EncodeHex(byteArray);

// Decode from Base32
byte[] decoded = Base32.Decode(encodedString);

// Decode from Base32Hex
byte[] hexDecoded = Base32.DecodeHex(hexEncodedString);
```

### Base64Extensions

Methods for converting between byte arrays and Base64 strings.

```csharp
// Convert bytes to Base64
string base64 = byteArray.ToBase64();

// Convert Base64 to bytes
byte[] bytes = base64String.FromBase64();

// Create a Base64 data URL for images
string dataUrl = imageBytes.ToBase64Image("image/png");
```

### CryptoRandom

Secure random number generation utilities.

```csharp
// Generate random bytes
byte[] randomBytes = CryptoRandom.GetRandomBytes(16);

// Generate a random string
string randomString = CryptoRandom.GetRandomString(32);

// Generate a random string with special characters
string randomWithSpecial = CryptoRandom.GetRandomString(32, true);
```

### EnumerableExtensions

Methods for working with enumerable collections.

```csharp
// Get collection or empty if null
var items = myCollection.OrEmpty();

// Traverse up a hierarchy
var ancestors = node.TraverseUpwards(n => n.Parent);

// Flatten a hierarchy
var allNodes = rootNode.FlattenHierarchy(n => n.Children);
```

### EnumExtensions

Methods for working with enumerations.

```csharp
// Convert enum values to snake_case strings
var snakeCaseValues = myEnum.ToSnakeCaseStrings();
```

### HexadecimalConverter

Methods for converting between byte arrays and hexadecimal strings.

```csharp
// Convert bytes to hexadecimal
string hex = byteArray.ToHexadecimalString();

// Convert hexadecimal to bytes
byte[] bytes = hexString.FromHexadecimalString();

// Convert numeric values to hexadecimal
string hexInt = HexadecimalConverter.ToHexadecimalString(42);
```

### HttpServerUtility

Methods for encoding and decoding URL tokens.

```csharp
// Encode bytes as a URL token
string token = HttpServerUtility.UrlTokenEncode(byteArray);

// Decode URL token to bytes
byte[] bytes = HttpServerUtility.UrlTokenDecode(token);
```

### NavigationManagerExtensions

Extensions for Blazor's NavigationManager.

```csharp
// Get a query parameter value
string value = navigationManager.GetQueryValue("paramName");
```

### ValidationContextExtensions

Extensions for working with validation contexts.

```csharp
// Get the display name for a validation context
string name = validationContext.GetName();
```

## Validation Attributes

### AbsoluteUriAttribute

Validates that a property, field, or parameter is an absolute URI.

```csharp
public class MyModel
{
    [AbsoluteUri]
    public string Website { get; set; }
    
    [AbsoluteUri(RequireScheme = "https")]
    public string SecureWebsite { get; set; }
}
```

### BeforeAtAttribute

Validates that a date value is before a specified date.

```csharp
public class Person
{
    [BeforeAt("now -18y")]  // Must be at least 18 years ago
    public DateTimeOffset BirthDate { get; set; }
    
    [BeforeAt("2030-01-01")]  // Must be before fixed date
    public DateTime ExpiryDate { get; set; }
}
```

## Certificate Management

### CertificateId

Record for identifying certificates by file paths.

```csharp
var certId = new CertificateId(
    CertPemFilePath: "/path/to/cert.pem",
    KeyPemFilePath: "/path/to/key.pem",
    Password: "optional-password"
);
```

### ICertificateProvider

Interface for retrieving X509 certificates based on certificate identifiers.

```csharp
public class MyCertificateProvider : ICertificateProvider
{
    public X509Certificate2 GetCertificate(CertificateId certificateId)
    {
        // Implementation to load certificate
    }
}
```

## Multi-Framework Support

This library targets both .NET 8.0 and .NET 9.0, with appropriate dependencies for each framework.