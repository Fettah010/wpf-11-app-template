namespace DevTemWpf.App.Services;

/// <summary>
/// Lifecycle owner for motion that is in flight (ADR-0016). Long-lived views
/// register the storyboards/timers they start and hand <see cref="Token"/> to
/// awaited primitives; the shell calls <see cref="StopAll"/> on
/// navigation-away, unload, theme change, deactivation, and shutdown so no
/// animation outlives its surface (and no rendering hook is ever leaked).
/// <see cref="StopAll"/> re-arms the scope for the next activation cycle;
/// <see cref="Dispose"/> retires it for good. Every callback is best-effort.
/// </summary>
public sealed class MotionScope : IDisposable
{
    private readonly List<Action> _stops = [];
    private CancellationTokenSource _cts = new();
    private bool _disposed;

    public bool IsDisposed => _disposed;

    /// <summary>Token for in-flight primitives; cancelled by StopAll/Dispose.</summary>
    public CancellationToken Token => _disposed ? new CancellationToken(canceled: true) : _cts.Token;

    /// <summary>True while the current cycle has been stopped (or the scope disposed).</summary>
    public bool IsStopped => _disposed || _cts.IsCancellationRequested;

    /// <summary>
    /// Registers a cleanup callback (typically stopping a looping storyboard).
    /// Dispose the returned handle to unregister it; after <see cref="Dispose"/>
    /// the callback runs immediately.
    /// </summary>
    public IDisposable Register(Action stop)
    {
        ArgumentNullException.ThrowIfNull(stop);
        if (_disposed)
        {
            stop();
            return NoOp.Instance;
        }

        _stops.Add(stop);
        return new Registration(this, stop);
    }

    /// <summary>Cancels in-flight primitives, runs registrations in reverse order, then re-arms.</summary>
    public void StopAll()
    {
        if (_disposed)
        {
            return;
        }

        CancelAndRun();
        _cts.Dispose();
        _cts = new CancellationTokenSource();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        CancelAndRun();
        _cts.Dispose();
    }

    private void CancelAndRun()
    {
        try
        {
            _cts.Cancel();
        }
        catch
        {
            // Cancellation is best effort.
        }

        for (var i = _stops.Count - 1; i >= 0; i--)
        {
            try
            {
                _stops[i]();
            }
            catch
            {
                // One broken cleanup must not block the rest.
            }
        }

        _stops.Clear();
    }

    private void Remove(Action stop) => _stops.Remove(stop);

    private sealed class Registration(MotionScope owner, Action stop) : IDisposable
    {
        private Action? _stop = stop;

        public void Dispose()
        {
            var stop = Interlocked.Exchange(ref _stop, null);
            if (stop is not null)
            {
                owner.Remove(stop);
            }
        }
    }

    private sealed class NoOp : IDisposable
    {
        public static readonly NoOp Instance = new();

        public void Dispose()
        {
        }
    }
}
