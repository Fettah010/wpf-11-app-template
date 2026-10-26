using System.Linq;
using Microsoft.Identity.Client;

namespace DevTemWpf.App.Services;

/// <summary>
/// MSAL (Entra ID) implementation of <see cref="IAuthTokenProvider"/>
/// (ADR-0017/D-2). Public client only — there is no client secret, no
/// embedded credential, and no network call until a configured user action
/// runs. Sample-scope: keep tokens in-process; do not persist them yourself.
/// </summary>
public sealed class MsalTokenProvider : IAuthTokenProvider
{
    public async Task<AuthResult> AcquireAsync(AuthOptions options, CancellationToken cancellationToken)
    {
        var app = Create(options);
        try
        {
            var result = await app
                .AcquireTokenInteractive(options.Scopes)
                .WithUseEmbeddedWebView(false)
                .ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);
            return new AuthResult(true, result.Account.Username, null);
        }
        catch (MsalException ex)
        {
            return AuthResult.Failed(ex.Message);
        }
    }

    public async Task<IReadOnlyList<string>> GetCachedAccountsAsync(AuthOptions options, CancellationToken cancellationToken)
    {
        var app = Create(options);
        var accounts = await app.GetAccountsAsync().ConfigureAwait(false);
        return accounts
            .Select(a => a.Username)
            .Where(u => !string.IsNullOrEmpty(u))
            .ToArray();
    }

    public async Task RemoveAccountsAsync(AuthOptions options, CancellationToken cancellationToken)
    {
        var app = Create(options);
        foreach (var account in await app.GetAccountsAsync().ConfigureAwait(false))
        {
            await app.RemoveAsync(account).ConfigureAwait(false);
        }
    }

    private static IPublicClientApplication Create(AuthOptions options) =>
        PublicClientApplicationBuilder
            .Create(options.ClientId)
            .WithAuthority($"{options.AuthorityHost.TrimEnd('/')}/{options.TenantId}")
            .WithRedirectUri(options.RedirectUri)
            .Build();
}
