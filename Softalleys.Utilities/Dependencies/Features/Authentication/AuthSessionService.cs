using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Softalleys.Utilities.Extensions;
using Softalleys.Utilities.ObjectValues.Authentication;

namespace Softalleys.Utilities.Dependencies.Features.Authentication;

/// <summary>
///     Manages user authentication, providing mechanisms to sign in, sign out, and maintain user sessions.
///     This interface plays a pivotal role in the security and session management of an application, ensuring that users
///     are
///     authenticated and their sessions are managed securely across different contexts and client applications.
/// </summary>
/// <param name="httpContextAccessor">Provides access to the current HTTP context.</param>
public class AuthSessionService(
    IHttpContextAccessor httpContextAccessor) : IAuthSessionService
{
	/// <summary>
	///     Provides direct access to the current <see cref="HttpContext" /> by ensuring it is available and not null.
	/// </summary>
	private HttpContext HttpContext =>
        httpContextAccessor.HttpContext.NotNull(nameof(IHttpContextAccessor.HttpContext));

	/// <summary>
	///     Asynchronously retrieves the current user's authentication session if available.
	///     This method wraps ASP.NET's built-in authentication mechanisms to provide an <see cref="AuthSession" /> model.
	/// </summary>
	/// <returns>
	///     An asynchronous stream of <see cref="AuthSession" /> instances representing the user's current
	///     authentication sessions.
	/// </returns>
	public async IAsyncEnumerable<AuthSession> GetAvailableAuthSessions()
    {
        var user = await AuthenticateAsync();
        if (user != null) yield return user;
    }

	/// <summary>
	///     Attempts to authenticate the current user based on the configured default authentication scheme,
	///     converting the authentication results into an <see cref="AuthSession" />.
	/// </summary>
	/// <returns>
	///     A task that represents the asynchronous operation. The task result contains the <see cref="AuthSession" />
	///     of the authenticated user or null if the authentication fails.
	/// </returns>
	public async Task<AuthSession?> AuthenticateAsync()
    {
        var authenticationResult = await HttpContext.AuthenticateAsync();
        if (!authenticationResult.Succeeded)
            return null;

        var principal = authenticationResult.Principal;
        var properties = authenticationResult.Properties;
        
        var jwt = properties.GetTokenValue(JwtBearerDefaults.AuthenticationScheme);

        var sessionId = jwt ?? Guid.NewGuid().ToString("N");
        
        var authenticationTime = properties.GetString("iat") ?? principal.FindFirstValue("iat");
        if (string.IsNullOrEmpty(authenticationTime))
            throw new InvalidOperationException("There is no iat in the properties");

        var authSession = new AuthSession(
            principal.FindFirstValue("sub").NotNull("sub").ToGuid(),
            sessionId,
            principal.FindFirstValue("iss").NotNull("iss"),
            principal.FindFirstValue("aud").NotNull("aud"),
            DateTimeOffset.FromUnixTimeSeconds(long.Parse(authenticationTime)),
            DateTimeOffset.FromUnixTimeSeconds(long.Parse(authenticationTime)))
        {
            PreferredUsername = principal.FindFirstValue("name") ?? principal.FindFirstValue("sub").NotNull("sub"),
            Username = principal.FindFirstValue("username").NotNull("username"),
            Email = principal.FindFirstValue("email") ?? string.Empty,

            Scopes = principal.Claims.Where(c => c.Type == "scope")
				.Select(c => c.Value)
				.ToArray(),
            Roles = principal.Claims.Where(c => c.Type == "role")
	            .Select(c => c.Value)
	            .ToArray()
        };

        return authSession;
    }

	/// <summary>
	///     Signs out the current user from the application, ending their authenticated session.
	/// </summary>
	/// <returns>A task that represents the asynchronous sign-out operation.</returns>
	public Task SignOutAsync()
    {
        return HttpContext.SignOutAsync();
    }

	/// <summary>
	///     Gets the identity provider used to authenticate the user.
	/// </summary>
	/// <returns></returns>
    public string GetIdentityProvider()
    {
	    return HttpContext.User.FindFirstValue("iss") ?? string.Empty;
    }
}