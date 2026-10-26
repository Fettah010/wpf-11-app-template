using DevTemWpf.App.Services;
using Microsoft.Extensions.Options;

namespace DevTemWpf.Unit;

/// <summary>
/// AC04: the auth sample is strictly opt-in, disabled without configuration,
/// and performs no work (no network, no provider call) when unconfigured.
/// </summary>
public sealed class AuthServiceTests
{
    private sealed class FakeProvider : IAuthTokenProvider
    {
        public int AcquireCalls { get; private set; }

        public int CachedCalls { get; private set; }

        public int RemoveCalls { get; private set; }

        public AuthResult Next { get; set; } = new(true, "user@example.com", null);

        public Exception? Throw { get; set; }

        public IReadOnlyList<string> Cached { get; set; } = [];

        public Task<AuthResult> AcquireAsync(AuthOptions options, CancellationToken cancellationToken)
        {
            AcquireCalls++;
            return Throw is null ? Task.FromResult(Next) : Task.FromException<AuthResult>(Throw);
        }

        public Task<IReadOnlyList<string>> GetCachedAccountsAsync(AuthOptions options, CancellationToken cancellationToken)
        {
            CachedCalls++;
            return Task.FromResult(Cached);
        }

        public Task RemoveAccountsAsync(AuthOptions options, CancellationToken cancellationToken)
        {
            RemoveCalls++;
            return Task.CompletedTask;
        }
    }

    private static AuthService Create(AuthOptions options, out FakeProvider provider)
    {
        provider = new FakeProvider();
        return new AuthService(Options.Create(options), provider);
    }

    [Fact]
    public void IsConfigured_RequiresClientAndTenant()
    {
        Assert.False(new AuthOptions().IsConfigured);
        Assert.False(new AuthOptions { ClientId = "client" }.IsConfigured);
        Assert.False(new AuthOptions { TenantId = "tenant" }.IsConfigured);
        Assert.False(new AuthOptions { ClientId = "  ", TenantId = "tenant" }.IsConfigured);
        Assert.True(new AuthOptions { ClientId = "client", TenantId = "tenant" }.IsConfigured);
    }

    [Fact]
    public async Task NotConfigured_SignIn_IsDisabledWithoutTouchingProvider()
    {
        var service = Create(new AuthOptions(), out var provider);

        var result = await service.SignInAsync(TestContext.Current.CancellationToken);

        Assert.False(service.IsConfigured);
        Assert.False(result.Success);
        Assert.Equal(0, provider.AcquireCalls);
        Assert.False(service.IsSignedIn);
    }

    [Fact]
    public async Task Configured_SignIn_SetsAccount()
    {
        var service = Create(new AuthOptions { ClientId = "client", TenantId = "tenant" }, out var provider);

        var result = await service.SignInAsync(TestContext.Current.CancellationToken);

        Assert.True(result.Success);
        Assert.True(service.IsSignedIn);
        Assert.Equal("user@example.com", service.AccountName);
        Assert.Equal(1, provider.AcquireCalls);
    }

    [Fact]
    public async Task SignIn_ProviderFailure_ReturnsErrorWithoutSigningIn()
    {
        var service = Create(new AuthOptions { ClientId = "client", TenantId = "tenant" }, out var provider);
        provider.Next = AuthResult.Failed("consent required");

        var result = await service.SignInAsync(TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.False(service.IsSignedIn);
        Assert.Contains("consent", result.Error);
    }

    [Fact]
    public async Task SignOut_ClearsAccount()
    {
        var service = Create(new AuthOptions { ClientId = "client", TenantId = "tenant" }, out var provider);
        await service.SignInAsync(TestContext.Current.CancellationToken);

        await service.SignOutAsync(TestContext.Current.CancellationToken);

        Assert.False(service.IsSignedIn);
        Assert.Null(service.AccountName);
        Assert.Equal(1, provider.RemoveCalls);
    }

    [Fact]
    public async Task TryRestore_UsesCachedAccountWithoutInteraction()
    {
        var service = Create(new AuthOptions { ClientId = "client", TenantId = "tenant" }, out var provider);
        provider.Cached = ["cached@example.com"];

        await service.TryRestoreAsync(TestContext.Current.CancellationToken);

        Assert.True(service.IsSignedIn);
        Assert.Equal("cached@example.com", service.AccountName);
        Assert.Equal(0, provider.AcquireCalls);
    }

    [Fact]
    public async Task TryRestore_NotConfigured_DoesNothing()
    {
        var service = Create(new AuthOptions(), out var provider);

        await service.TryRestoreAsync(TestContext.Current.CancellationToken);

        Assert.False(service.IsSignedIn);
        Assert.Equal(0, provider.CachedCalls);
    }
}
