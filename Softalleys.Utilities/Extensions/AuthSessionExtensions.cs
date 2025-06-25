using Softalleys.Utilities.ObjectValues.Authentication;

namespace Softalleys.Utilities.Extensions;

/// <summary>
/// Extension methods for <see cref="AuthSession"/> to check scopes and roles.
/// </summary>
public static class AuthSessionExtensions
{
    /// <summary>
    /// Determines whether the session contains the specified scope.
    /// </summary>
    /// <param name="session">The authentication session.</param>
    /// <param name="scope">The scope to check for.</param>
    /// <returns><c>true</c> if the session contains the scope; otherwise, <c>false</c>.</returns>
    public static bool HasScope(this AuthSession? session, string scope)
    {
        if (session is null || string.IsNullOrWhiteSpace(scope))
        {
            return false;
        }

        return session.Scopes.Contains(scope);
    }

    /// <summary>
    /// Determines whether the session contains all of the specified scopes.
    /// </summary>
    /// <param name="session">The authentication session.</param>
    /// <param name="scopes">The scopes to check for.</param>
    /// <returns><c>true</c> if the session contains all scopes; otherwise, <c>false</c>.</returns>
    public static bool HasScopes(this AuthSession? session, params string[] scopes)
    {
        if (session is null)
        {
            return false;
        }

        if (scopes.Length == 0 && session.Scopes.Count > 0)
        {
            return true; // If no scopes are provided, but session has scopes, return true
        }

        if (scopes.Length == 0 || session.Scopes.Count == 0)
        {
            return false; // If no scopes are provided or session has no scopes, return false
        }

        return scopes.All(scope => session.Scopes.Contains(scope));
    }

    /// <summary>
    /// Determines whether the session contains any of the specified scopes.
    /// </summary>
    /// <param name="session">The authentication session.</param>
    /// <param name="scopes">The scopes to check for.</param>
    /// <returns><c>true</c> if the session contains any scope; otherwise, <c>false</c>.</returns>
    public static bool HasAnyScope(this AuthSession? session, params string[] scopes)
    {
        if (session is null)
        {
            return false;
        }

        if (scopes.Length == 0 && session.Scopes.Count > 0)
        {
            return true; // If no scopes are provided, but session has scopes, return true
        }

        if (scopes.Length == 0 || session.Scopes.Count == 0)
        {
            return false; // If no scopes are provided or session has no scopes, return false
        }

        return scopes.Any(scope => session.Scopes.Contains(scope));
    }

    /// <summary>
    /// Determines whether the session contains a scope that ends with the specified string (case-insensitive).
    /// </summary>
    /// <param name="session">The authentication session.</param>
    /// <param name="scope">The scope ending to check for.</param>
    /// <returns><c>true</c> if any scope ends with the specified string; otherwise, <c>false</c>.</returns>
    public static bool HasScopeThatEndsWith(this AuthSession? session, string scope)
    {
        if (session is null || string.IsNullOrWhiteSpace(scope))
        {
            return false;
        }

        return session.Scopes.Any(s => s.EndsWith(scope, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Determines whether the session contains a scope that starts with the specified string (case-insensitive).
    /// </summary>
    /// <param name="session">The authentication session.</param>
    /// <param name="scope">The scope prefix to check for.</param>
    /// <returns><c>true</c> if any scope starts with the specified string; otherwise, <c>false</c>.</returns>
    public static bool HasScopeThatStartsWith(this AuthSession? session, string scope)
    {
        if (session is null || string.IsNullOrWhiteSpace(scope))
        {
            return false;
        }

        return session.Scopes.Any(s => s.StartsWith(scope, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Determines whether the session contains a scope that contains the specified string (case-insensitive).
    /// </summary>
    /// <param name="session">The authentication session.</param>
    /// <param name="scope">The substring to check for in scopes.</param>
    /// <returns><c>true</c> if any scope contains the specified string; otherwise, <c>false</c>.</returns>
    public static bool HasScopeThatContains(this AuthSession? session, string scope)
    {
        if (session is null || string.IsNullOrWhiteSpace(scope))
        {
            return false;
        }

        return session.Scopes.Any(s => s.Contains(scope, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Determines whether the session contains the specified role.
    /// </summary>
    /// <param name="session">The authentication session.</param>
    /// <param name="role">The role to check for.</param>
    /// <returns><c>true</c> if the session contains the role; otherwise, <c>false</c>.</returns>
    public static bool HasRole(this AuthSession? session, string role)
    {
        if (session is null || string.IsNullOrWhiteSpace(role))
        {
            return false;
        }

        return session.Roles.Contains(role);
    }

    /// <summary>
    /// Determines whether the session contains all of the specified roles.
    /// </summary>
    /// <param name="session">The authentication session.</param>
    /// <param name="roles">The roles to check for.</param>
    /// <returns><c>true</c> if the session contains all roles; otherwise, <c>false</c>.</returns>
    public static bool HasRoles(this AuthSession? session, params string[] roles)
    {
        if (session is null)
        {
            return false;
        }

        if (roles.Length == 0 && session.Roles.Count > 0)
        {
            return true; // If no roles are provided, but session has roles, return true
        }

        if (roles.Length == 0 || session.Roles.Count == 0)
        {
            return false; // If no roles are provided or session has no roles, return false
        }

        return roles.All(role => session.Roles.Contains(role));
    }

    /// <summary>
    /// Determines whether the session contains any of the specified roles.
    /// </summary>
    /// <param name="session">The authentication session.</param>
    /// <param name="roles">The roles to check for.</param>
    /// <returns><c>true</c> if the session contains any role; otherwise, <c>false</c>.</returns>
    public static bool HasAnyRole(this AuthSession? session, params string[] roles)
    {
        if (session is null || roles.Length == 0)
        {
            return false;
        }

        if (roles.Length == 0 && session.Roles.Count > 0)
        {
            return true; // If no roles are provided, but session has roles, return true
        }

        if (roles.Length == 0 || session.Roles.Count == 0)
        {
            return false; // If no roles are provided or session has no roles, return false
        }

        return roles.Any(role => session.Roles.Contains(role));
    }

    /// <summary>
    /// Determines whether the session contains a role that ends with the specified string (case-insensitive).
    /// </summary>
    /// <param name="session">The authentication session.</param>
    /// <param name="role">The role ending to check for.</param>
    /// <returns><c>true</c> if any role ends with the specified string; otherwise, <c>false</c>.</returns>
    public static bool HasRoleThatEndsWith(this AuthSession? session, string role)
    {
        if (session is null || string.IsNullOrWhiteSpace(role))
        {
            return false;
        }

        return session.Roles.Any(r => r.EndsWith(role, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Determines whether the session contains a role that starts with the specified string (case-insensitive).
    /// </summary>
    /// <param name="session">The authentication session.</param>
    /// <param name="role">The role prefix to check for.</param>
    /// <returns><c>true</c> if any role starts with the specified string; otherwise, <c>false</c>.</returns>
    public static bool HasRoleThatStartsWith(this AuthSession? session, string role)
    {
        if (session is null || string.IsNullOrWhiteSpace(role))
        {
            return false;
        }

        return session.Roles.Any(r => r.StartsWith(role, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Determines whether the session contains a role that contains the specified string (case-insensitive).
    /// </summary>
    /// <param name="session">The authentication session.</param>
    /// <param name="role">The substring to check for in roles.</param>
    /// <returns><c>true</c> if any role contains the specified string; otherwise, <c>false</c>.</returns>
    public static bool HasRoleThatContains(this AuthSession? session, string role)
    {
        if (session is null || string.IsNullOrWhiteSpace(role))
        {
            return false;
        }

        return session.Roles.Any(r => r.Contains(role, StringComparison.OrdinalIgnoreCase));
    }
}