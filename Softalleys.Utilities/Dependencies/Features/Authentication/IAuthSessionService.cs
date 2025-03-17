using Softalleys.Utilities.ObjectValues.Authentication;

namespace Softalleys.Utilities.Dependencies.Features.Authentication;

/// <summary>
///     Manages user authentication, providing mechanisms to sign in, sign out, and maintain user sessions.
///     This interface plays a pivotal role in the security and session management of an application, ensuring that users
///     are
///     authenticated and their sessions are managed securely across different contexts and client applications.
/// </summary>
public interface IAuthSessionService
{
    /// <summary>
    ///     Retrieves currently active authentication sessions for the user.
    ///     This method is typically used to display all sessions that a user has, allowing them to manage their sessions.
    /// </summary>
    /// <returns>An asynchronous stream of <see cref="AuthSession" /> instances, each representing an active user session.</returns>
    IAsyncEnumerable<AuthSession> GetAvailableAuthSessions();

    /// <summary>
    ///     Authenticates the current user based on the session context, verifying their identity and session validity.
    ///     This method is crucial for ensuring that requests are made by an authenticated user and for retrieving the user's
    ///     session information.
    /// </summary>
    /// <returns>
    ///     A task that resolves to an <see cref="AuthSession" /> representing the authenticated user's session, or null if no
    ///     valid session exists.
    /// </returns>
    Task<AuthSession?> AuthenticateAsync();

    /// <summary>
    ///     Terminates the current user session, effectively signing out the user.
    ///     This method is crucial for maintaining the security of the application by ensuring that user sessions can be
    ///     properly closed.
    /// </summary>
    /// <returns>A task that signifies the completion of the user sign-out process.</returns>
    Task SignOutAsync();

    /// <summary>
    ///     Gets the identity provider used to authenticate the user.
    /// </summary>
    /// <returns></returns>
    string GetIdentityProvider();
}