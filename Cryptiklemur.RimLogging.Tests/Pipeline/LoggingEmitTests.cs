using System;
using System.Collections.Generic;
using System.Threading;
using Cryptiklemur.RimLogging;
using Xunit;

namespace Cryptiklemur.RimLogging.Tests.Pipeline;

public class LoggingEmitTests : IDisposable
{
    private readonly LogLevel _savedMin;
    private readonly Action<LogEntry>? _savedOverride;

    public LoggingEmitTests()
    {
        _savedMin = Logging.GlobalMinLevel;
        _savedOverride = Logging._dispatchSyncOverride;
        Logging.GlobalMinLevel = LogLevel.Trace;
    }

    public void Dispose()
    {
        Logging.StopForTests();
        Logging.GlobalMinLevel = _savedMin;
        Logging._dispatchSyncOverride = _savedOverride;
    }

    [Fact]
    public void Error_BypassesQueue_DispatchesOnCallingThread()
    {
        int callingThreadId = Thread.CurrentThread.ManagedThreadId;
        int dispatchedThreadId = -1;

        Logging._dispatchSyncOverride = e => dispatchedThreadId = Thread.CurrentThread.ManagedThreadId;

        Log.Error("sync-bypass-test");

        Assert.Equal(callingThreadId, dispatchedThreadId);
    }

    [Fact]
    public void Info_WithDrainStarted_DispatchesOnDrainThread()
    {
        int callingThreadId = Thread.CurrentThread.ManagedThreadId;
        ManualResetEventSlim dispatched = new ManualResetEventSlim(false);
        int dispatchedThreadId = -1;

        Logging._dispatchSyncOverride = e =>
        {
            dispatchedThreadId = Thread.CurrentThread.ManagedThreadId;
            dispatched.Set();
        };

        Logging.EnsureStarted();
        Log.Info("async-drain-test");

        bool signaled = dispatched.Wait(5000);
        Assert.True(signaled, "Drain thread did not dispatch within 5 s");
        Assert.NotEqual(callingThreadId, dispatchedThreadId);
    }
}
