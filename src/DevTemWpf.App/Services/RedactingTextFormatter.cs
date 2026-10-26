using System.IO;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace DevTemWpf.App.Services;

/// <summary>
/// Wraps the message-template formatter and scrubs secret-shaped text from
/// every emitted line (1.6.0, ADR-0018). Formatting is defensive: a hostile
/// message, property, or exception must never take down logging.
/// </summary>
public sealed class RedactingTextFormatter : ITextFormatter
{
    private readonly MessageTemplateTextFormatter _inner;

    public RedactingTextFormatter(string outputTemplate)
        => _inner = new MessageTemplateTextFormatter(outputTemplate);

    public void Format(LogEvent logEvent, TextWriter output)
    {
        try
        {
            using var buffer = new StringWriter();
            _inner.Format(logEvent, buffer);
            output.Write(LogPrivacy.Redact(buffer.ToString()));
        }
        catch (Exception ex)
        {
            // Safe exception rendering: never propagate a formatting failure.
            output.Write(LogPrivacy.Redact(
                $"{logEvent.Timestamp:o} [FTL] log-format-failed {ex.GetType().Name}: {ex.Message}"));
        }
    }
}
