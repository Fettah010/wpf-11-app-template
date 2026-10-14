using System.Net.Http;
using Microsoft.Extensions.Options;

namespace DevTemWpf.App.Services;

/// <summary>
/// Typed HTTP boundary: configured base URL, timeout, cancellation, and
/// mapped errors. Makes zero calls unless a base URL is configured.
/// </summary>
public sealed class StatusClient
{
    private readonly HttpClient _http;
    private readonly HttpOptions _options;

    public StatusClient(HttpClient http, IOptions<HttpOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.BaseUrl);

    public async Task<(bool Ok, string Detail)> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return (false, "HTTP client is not configured (empty base URL).");
        }

        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(Math.Max(1, _options.TimeoutSeconds)));
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
            using var response = await _http.GetAsync("status", linked.Token).ConfigureAwait(false);
            var body = (await response.Content.ReadAsStringAsync(linked.Token).ConfigureAwait(false)).Trim();
            return response.IsSuccessStatusCode
                ? (true, string.IsNullOrEmpty(body) ? $"OK ({(int)response.StatusCode})." : body)
                : (false, $"Server returned {(int)response.StatusCode}.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return (false, $"Timed out after {_options.TimeoutSeconds}s.");
        }
        catch (OperationCanceledException)
        {
            return (false, "Cancelled.");
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Request failed: {ex.Message}");
        }
    }
}
