using System.ComponentModel.DataAnnotations;
using Softalleys.Utilities.Extensions;

namespace Softalleys.Utilities.Attributes;

/// <summary>
///     A validation attribute that ensures a property, field, or parameter is an absolute URI.
///     Optionally, it can also require a specific URI scheme.
/// </summary>
/// <remarks>
///     This attribute can be applied to properties, fields, or parameters that are expected to represent absolute URIs.
///     If a specific scheme is required, it can be set using the <see cref="RequireScheme" /> property.
///     The absence of a value is considered valid; use the
///     <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute" />
///     in combination with this attribute to enforce non-null/empty values.
/// </remarks>
public sealed class AbsoluteUriAttribute : ValidationAttribute
{
	/// <summary>
	///     Gets or sets the URI scheme that the URI must use, such as "http" or "https".
	///     If not set, any absolute URI scheme is considered valid.
	/// </summary>
	public string? RequireScheme { get; set; }

	/// <summary>
	///     Determines whether the specified value of the object is valid.
	/// </summary>
	/// <param name="value">The value of the object to validate.</param>
	/// <param name="validationContext">The context information about the object being validated.</param>
	/// <returns>
	///     <see cref="ValidationResult.Success" /> if the value is a valid absolute URI, otherwise an error
	///     <see cref="ValidationResult" />.
	/// </returns>
	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        return value switch
        {
            null => ValidationResult.Success, // absence is OK, apply [Required] if needed

            string str when string.IsNullOrEmpty(str) => ValidationResult.Success,
            Uri uri when string.IsNullOrEmpty(uri.OriginalString) => ValidationResult.Success,

            string str when Uri.TryCreate(str, UriKind.RelativeOrAbsolute, out var uri) => IsValid(uri,
                validationContext),

            Uri { IsAbsoluteUri: true, Scheme: var scheme } when RequireScheme.HasValue() &&
                                                                 !string.Equals(RequireScheme, scheme,
                                                                     StringComparison.OrdinalIgnoreCase)
                => new ValidationResult($"{validationContext.GetName()} value must use {RequireScheme} scheme."),

            Uri { IsAbsoluteUri: true } => ValidationResult.Success,

            Uri => new ValidationResult($"{validationContext.GetName()} value is not absolute."),
            _ => new ValidationResult($"{validationContext.GetName()} is not Uri, but {value.GetType().Name}.")
        };
    }
}