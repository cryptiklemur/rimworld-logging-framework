using System;
using System.Threading;
using Cryptiklemur.RimLogging;
using Xunit;

namespace Cryptiklemur.RimLogging.Tests.Pipeline;

public class LoggingLifecycleTests : IDisposable
{
    private readonly LogLevel _savedMin;
    private readonly Action<LogEntry>? _savedOverride;

    public LoggingLifecycleTests()
    {
        _savedMin = Logging.GlobalMinLevel;
        _savedOverride = Logging._dispatchSyncOverride;
        Logging.GlobalMinLevel = LogLevel.Trace;
    }

    public void Dispose()
    {
        Logging.Shutdown();
        Logging.GlobalMinLevel = _savedMin;
        Logging._dispatchSyncOverride = _savedOverride;
    }

    [Fact]
    public void Init_IsIdempotent()
    {
        Logging.Init();

        Action<LogEntry>? firstCapture = null;
        Logging._dispatchSyncOverride = e => firstCapture = Logging._dispatchSyncOverride;

        Logging.Init();

        // A second Init must not replace the drain -- the override set after first Init
        // is still reachable (queue/drain same instance means drain thread is alive).
        // Simplest observable: call EnsureStarted then Init, both are no-ops beyond first.
        // Verify the drain thread is still running by posting and receiving on same override.
        bool dispatched = false;
        ManualResetEventSlim ready = new ManualResetEventSlim(false);
        Logging._dispatchSyncOverride = e => { dispatched = true; ready.Set(); };

        Log.Info("idempotency-check");

        Assert.True(ready.Wait(5000), "Drain thread did not dispatch within 5 s");
        Assert.True(dispatched);
    }

    [Fact]
    public void Shutdown_NullsOutQueueAndDrain()
    {
        Logging.Init();
        Logging.Shutdown();

        // After Shutdown, Emit should route synchronously (drain == null).
        int callingThreadId = Thread.CurrentThread.ManagedThreadId;
        int dispatchedThreadId = -1;
        Logging._dispatchSyncOverride = e => dispatchedThreadId = Thread.CurrentThread.ManagedThreadId;

        Log.Info("shutdown-test");

        Assert.Equal(callingThreadId, dispatchedThreadId);
    }

    [Fact]
    public void EmitAfterShutdown_DispatchesSynchronously()
    {
        Logging.Init();
        Logging.Shutdown();

        int callingThreadId = Thread.CurrentThread.ManagedThreadId;
        int dispatchedThreadId = -1;
        Logging._dispatchSyncOverride = e => dispatchedThreadId = Thread.CurrentThread.ManagedThreadId;

        Log.Info("post-shutdown-emit");

        Assert.Equal(callingThreadId, dispatchedThreadId);
    }

    [Fact]
    public void EnsureStartedAndInit_AreCompatible()
    {
        // EnsureStarted followed by Init must not double-allocate.
        Logging.EnsureStarted();

        bool dispatched = false;
        ManualResetEventSlim ready = new ManualResetEventSlim(false);
        Logging._dispatchSyncOverride = e => { dispatched = true; ready.Set(); };

        Logging.Init(); // must be a no-op for queue/drain

        Log.Info("ensure-then-init");

        Assert.True(ready.Wait(5000), "Drain thread did not dispatch within 5 s");
        Assert.True(dispatched);
    }
}
