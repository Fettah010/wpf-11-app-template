using System.IO;
using DevTemWpf.App.Services;
using Serilog.Events;
using Serilog.Parsing;

namespace DevTemWpf.Unit;

public sealed class LogPrivacyTests
{
    [Theory]
    [InlineData("login password=hunter2", "password=***")]
    [InlineData("token: abc123def", "token: ***")]
    [InlineData("apiKey=SECRETVALUE", "apiKey=***")]
    [InlineData("ConnectionString=Server=db;Secret=x", "ConnectionString=***")]
    [InlineData("client_secret = \"shhh\"", "client_secret = \"***\"")]
    public void Redact_MasksSecretKeyValues(string input, string expectedFragment)
    {
        var redacted = LogPrivacy.Redact(input);

        Assert.Contains(expectedFragment, redacted);
    }

    [Fact]
    public void Redact_MasksBearerToken()
    {
        var redacted = LogPrivacy.Redact("Sent header Bearer eyJhbGciOiJIUzI1NiJ9.payload.sig");

        Assert.Contains("Bearer ***", redacted);
        Assert.DoesNotContain("eyJhbGciOiJIUzI1NiJ9", redacted);
    }

    [Fact]
    public void Redact_LeavesOrdinaryTextUntouched()
    {
        const string input = "Loaded 42 rows from the dashboard in 12 ms";

        Assert.Equal(input, LogPrivacy.Redact(input));
    }

    [Fact]
    public void Redact_HandlesNull()
    {
        Assert.Equal(string.Empty, LogPrivacy.Redact(null));
    }

    [Fact]
    public void Formatter_RedactsSecretInFormattedLine()
    {
        var logEvent = BuildEvent("password={Pw}", new LogEventProperty("Pw", new ScalarValue("hunter2")));
        var rendered = Format(logEvent);

        Assert.Contains("password=***", rendered);
        Assert.DoesNotContain("hunter2", rendered);
    }

    [Fact]
    public void Formatter_PlainMessageIsUnchanged()
    {
        var logEvent = BuildEvent("Started {Name}", new LogEventProperty("Name", new ScalarValue("DevTemWpf")));
        var rendered = Format(logEvent);

        Assert.Contains("Started DevTemWpf", rendered);
    }

    [Fact]
    public void Formatter_NeverThrowsOnHostileEvent()
    {
        var formatter = new RedactingTextFormatter("{Message:lj}");
        var template = new MessageTemplateParser().Parse("boom");
        var logEvent = new LogEvent(
            DateTimeOffset.UnixEpoch, LogEventLevel.Error, new InvalidOperationException("kaboom"),
            template, []);

        var ex = Record.Exception(() =>
        {
            using var writer = new StringWriter();
            formatter.Format(logEvent, writer);
        });

        Assert.Null(ex);
    }

    private static string Format(LogEvent logEvent)
    {
        var formatter = new RedactingTextFormatter("{Message:lj}");
        using var writer = new StringWriter();
        formatter.Format(logEvent, writer);
        return writer.ToString();
    }

    private static LogEvent BuildEvent(string template, LogEventProperty property)
    {
        var parsed = new MessageTemplateParser().Parse(template);
        return new LogEvent(
            DateTimeOffset.UnixEpoch, LogEventLevel.Information, null, parsed, [property]);
    }
}
