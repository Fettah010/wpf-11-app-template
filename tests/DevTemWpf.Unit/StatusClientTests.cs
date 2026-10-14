using System.Net;
using DevTemWpf.App.Services;
using Microsoft.Extensions.Options;

namespace DevTemWpf.Unit;

public sealed class StatusClientTests
{
    private sealed class StubHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, HttpResponseMessage> Responder { get; set; }
            = _ => new HttpResponseMessage(HttpStatusCode.OK);

        public Exception? Throw { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (Throw is not null)
            {
                throw Throw;
            }

            return Task.FromResult(Responder(request));
        }
    }

    private static StatusClient Create(StubHandler handler, string baseUrl = "https://example.test/")
        => new(new HttpClient(handler) { BaseAddress = new Uri(baseUrl) },
            Options.Create(new HttpOptions { BaseUrl = baseUrl }));

    [Fact]
    public async Task Unconfigured_ReturnsDisabled_WithoutNetwork()
    {
        var handler = new StubHandler();
        var client = new StatusClient(new HttpClient(handler),
            Options.Create(new HttpOptions { BaseUrl = string.Empty }));

        var (ok, detail) = await client.GetStatusAsync(TestContext.Current.CancellationToken);

        Assert.False(ok);
        Assert.Contains("not configured", detail);
    }

    [Fact]
    public async Task Success_ReturnsBody()
    {
        var handler = new StubHandler
        {
            Responder = _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("all good"),
            },
        };

        var (ok, detail) = await Create(handler).GetStatusAsync(TestContext.Current.CancellationToken);

        Assert.True(ok);
        Assert.Equal("all good", detail);
    }

    [Fact]
    public async Task Timeout_MapsToFriendlyMessage()
    {
        var handler = new StubHandler { Throw = new TaskCanceledException("simulated") };

        var (ok, detail) = await Create(handler).GetStatusAsync(TestContext.Current.CancellationToken);

        Assert.False(ok);
        Assert.Contains("Timed out", detail);
    }

    [Fact]
    public async Task ConnectionFailure_MapsToFriendlyMessage()
    {
        var handler = new StubHandler { Throw = new HttpRequestException("no route") };

        var (ok, detail) = await Create(handler).GetStatusAsync(TestContext.Current.CancellationToken);

        Assert.False(ok);
        Assert.Contains("no route", detail);
    }
}
