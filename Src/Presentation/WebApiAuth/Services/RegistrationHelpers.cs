namespace WebApiAuth.Services;

/// <summary>
/// Shared bits of the invite/reset token handling, used by both RegisterUsersController and
/// ResetPasswordController so the two cannot drift apart.
/// </summary>
public static class RegistrationHelpers
{
    /// <summary>
    /// <c>RegisterTokenExpieryTime</c> is stored as UTC (<c>UtcNow.AddHours(2)</c>), so it has to be
    /// compared against UtcNow. The old check subtracted <c>DateTime.Now</c> and asked for whole
    /// days, which never reached 1 inside a two-hour window — the expiry was never enforced.
    /// </summary>
    public static bool HasExpired(DateTime expiryTimeUtc) => DateTime.UtcNow > expiryTimeUtc;


    /// <summary>
    /// Keycloak's user profile requires a first and last name, but the domain only carries FullName.
    /// Split on the last space; a single-word name becomes the first name with the whole name
    /// repeated as the last, since neither may be empty.
    /// </summary>
    public static (string FirstName, string LastName) SplitFullName(string fullName)
    {
        var trimmed = (fullName ?? string.Empty).Trim();

        if (trimmed.Length == 0)
            return ("Unknown", "User");

        var lastSpace = trimmed.LastIndexOf(' ');

        return lastSpace <= 0
            ? (trimmed, trimmed)
            : (trimmed[..lastSpace].Trim(), trimmed[(lastSpace + 1)..].Trim());
    }
}
