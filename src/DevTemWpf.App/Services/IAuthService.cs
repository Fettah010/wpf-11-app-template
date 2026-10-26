namespace DevTemWpf.App.Services;

/// <summary>Outcome of an interactive sign-in attempt.</summary>
/// <param name="Success">True when a token/account was obtained.</param>
/// <param name="AccountName">Friendly account name when known.</param>
/// <param name="Error">User-safe error text when <paramref name="Success"/> is false.</param>
public sealed record AuthResult(bool Success, string? AccountName, string? Error)
{
    public static AuthResult NotConfigured() =>
        new(false, null, "Authentication is not configured.");

    public static AuthResult Failed(string error) => new(false, null, error);
}

/// <summary>Seam over the token library so the flow is unit-testable (ADR-0017).</summary>
public interface IAuthTokenProvider
{
    /// <summary>Runs interactive sign-in and returns the account name (or error).</summary>
    Task<AuthResult> AcquireAsync(AuthOptions options, CancellationToken cancellationToken);

    /// <summary>Cached account names, without any network/interaction.</summary>
    Task<IReadOnlyList<string>> GetCachedAccountsAsync(AuthOptions options, CancellationToken cancellationToken);

    /// <summary>Removes cached accounts (sign-out).</summary>
    Task RemoveAccountsAsync(AuthOptions options, CancellationToken cancellationToken);
}

/// <summary>
/// Optional auth sample (ADR-0017/D-2): strictly opt-in (Custom preset only),
/// disabled without configuration, no embedded secrets, no default telemetry.
/// </summary>
public interface IAuthService
{
    /// <summary>True only when <see cref="AuthOptions"/> has a client + tenant.</summary>
    bool IsConfigured { get; }

    bool IsSignedIn { get; }

    string? AccountName { get; }

    Task<AuthResult> SignInAsync(CancellationToken cancellationToken = default);

    Task SignOutAsync(CancellationToken cancellationToken = default);

    /// <summary>Silently restores a previously cached account (no interaction).</summary>
    Task TryRestoreAsync(CancellationToken cancellationToken = default);
}
