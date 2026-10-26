namespace DevTemWpf.App.Services;

/// <summary>
/// Configuration for the optional MSAL (Entra ID) auth sample (ADR-0017/D-2).
/// Public-client values only — <b>never</b> a client secret. Bind from the
/// <c>Auth</c> configuration section or environment
/// (<c>Auth__ClientId</c>, <c>Auth__TenantId</c>). Empty by default, so the
/// sample is inert until an operator configures it.
/// </summary>
public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>Entra ID application (client) id. Not a secret.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>Entra ID tenant id or verified domain. Not a secret.</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Authority host; sovereign clouds override this.</summary>
    public string AuthorityHost { get; set; } = "https://login.microsoftonline.com";

    /// <summary>Delegated scopes requested at sign-in.</summary>
    public string[] Scopes { get; set; } = ["User.Read"];

    /// <summary>True only when a client and tenant are present.</summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(TenantId);

    /// <summary>Interactive redirect URI (loopback) for the desktop broker.</summary>
    public string RedirectUri { get; set; } = "http://localhost";
}
