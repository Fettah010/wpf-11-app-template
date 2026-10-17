using Microsoft.Extensions.Options;

namespace DevTemWpf.App.Services;

/// <summary>
/// Default <see cref="IAuthService"/>. All work is delegated to an
/// <see cref="IAuthTokenProvider"/> (MSAL in production), and every path
/// short-circuits to a no-op result when unconfigured — so an unconfigured
/// scaffold performs <b>no</b> network activity (ADR-0017/D-2).
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IOptions<AuthOptions> _options;
    private readonly IAuthTokenProvider _provider;

    public AuthService(IOptions<AuthOptions> options, IAuthTokenProvider? provider = null)
    {
        _options = options;
        _provider = provider ?? new MsalTokenProvider();
    }

    public bool IsConfigured => _options.Value.IsConfigured;

    public bool IsSignedIn { get; private set; }

    public string? AccountName { get; private set; }

    public async Task<AuthResult> SignInAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return AuthResult.NotConfigured();
        }

        try
        {
            var result = await _provider.AcquireAsync(_options.Value, cancellationToken).ConfigureAwait(false);
            if (result.Success)
            {
                IsSignedIn = true;
                AccountName = result.AccountName;
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            return AuthResult.Failed("Sign-in was cancelled.");
        }
        catch (Exception ex)
        {
            return AuthResult.Failed(ex.Message);
        }
    }

    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        if (IsConfigured)
        {
            try
            {
                await _provider.RemoveAccountsAsync(_options.Value, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // Sign-out is best-effort: local state is cleared regardless.
            }
        }

        IsSignedIn = false;
        AccountName = null;
    }

    public async Task TryRestoreAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConfigured || IsSignedIn)
        {
            return;
        }

        try
        {
            var accounts = await _provider.GetCachedAccountsAsync(_options.Value, cancellationToken).ConfigureAwait(false);
            if (accounts.Count > 0)
            {
                IsSignedIn = true;
                AccountName = accounts[0];
            }
        }
        catch
        {
            // Restore is opportunistic; failure simply means "not signed in".
        }
    }
}
