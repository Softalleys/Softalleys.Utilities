using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Softalleys.Utilities.Extensions;

/// <summary>
///     Provides extension methods for <see cref="ValidationContext" />.
/// </summary>
public static class ValidationContextExtensions
{
    /// <summary>
    ///     Retrieves the name of the member associated with the validation context,
    ///     taking into account custom attributes that might alter the displayed name.
    /// </summary>
    /// <param name="context">The <see cref="ValidationContext" /> instance.</param>
    /// <returns>
    ///     The name of the member for display purposes. This could be the value set in
    ///     <see cref="JsonPropertyNameAttribute" />, if they are applied.
    ///     Otherwise, it defaults to the display name provided by the context.
    /// </returns>
    /// <remarks>
    ///     <see cref="JsonPropertyNameAttribute" /> applied, and if so, returns the specified name.
    ///     If these attributes are not present, it falls back to the default display name.
    /// </remarks>
    public static string? GetName(this ValidationContext context)
    {
        if (context.MemberName == null)
            return null;

        var member = context.ObjectType.GetMember(context.MemberName).SingleOrDefault();

        return member switch
        {
            not null when member.GetCustomAttribute<JsonPropertyNameAttribute>() is { Name: var name } => name,
            _ => context.DisplayName
        };
    }
}