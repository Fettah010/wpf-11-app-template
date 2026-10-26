using System.Text.RegularExpressions;

namespace DevTemWpf.App.Services;

/// <summary>
/// Privacy helpers for the file log (1.6.0, ADR-0018). Redaction is always on
/// for the app log: secret-shaped key/value text (passwords, tokens, API keys,
/// connection strings, DSNs, bearer tokens) is masked before a line is
/// written. This is a defence-in-depth scrub, not a guarantee — call sites
/// must still avoid logging raw secrets (see docs/guides/diagnostics.md).
/// </summary>
public static partial class LogPrivacy
{
    /// <summary>Replacement written in place of a detected secret value.</summary>
    public const string Mask = "***";

    // key: value / key=value / "key": "value" for secret-named keys.
    [GeneratedRegex(
        "(?<key>password|passwd|pwd|secret|token|api[_-]?key|access[_-]?token|refresh[_-]?token|authorization|connection[_-]?string|dsn|client[_-]?secret)(?<sep>\\s*[\"']?\\s*[:=]\\s*[\"']?)(?<val>[^\\s\"';,}\\]]+)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SecretKeyValue();

    // Header-style bearer credentials.
    [GeneratedRegex("Bearer\\s+[A-Za-z0-9\\-._~+/]+=*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex Bearer();

    /// <summary>Masks secret-shaped values in a rendered log line.</summary>
    public static string Redact(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text ?? string.Empty;
        }

        var result = Bearer().Replace(text, "Bearer " + Mask);
        result = SecretKeyValue().Replace(
            result,
            static m => string.Concat(m.Groups["key"].Value, m.Groups["sep"].Value, Mask));
        return result;
    }
}
