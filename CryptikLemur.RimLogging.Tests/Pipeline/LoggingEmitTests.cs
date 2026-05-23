using System;
using System.Threading;
using CryptikLemur.RimLogging;
using CryptikLemur.RimLogging.Sinks;
using Xunit;

namespace CryptikLemur.RimLogging.Tests.Pipeline;

public class LoggingEmitTests : IDisposable
{
    private readonly LogLevel _savedMin;
    private readonly MemoryLogSink _sink = new MemoryLogSink();

    public LoggingEmitTests()
    {
        _savedMin = Logging.GlobalMinLevel;
        SinkRegistry.DisposeAll();
        SinkRegistry.Register(_sink);
        Logging.GlobalMinLevel = LogLevel.Trace;
    }

    public void Dispose()
    {
        Logging.StopForTests();
        Logging.GlobalMinLevel = _savedMin;
        SinkRegistry.Remove(_sink);
        _sink.Dispose();
    }

    [Fact]
    public void Error_BypassesQueue_DispatchesOnCallingThread()
    {
        int callingThreadId = Thread.CurrentThread.ManagedThreadId;
        int dispatchedThreadId = -1;
        ThreadCaptureSink captureSink = new ThreadCaptureSink(id => dispatchedThreadId = id);
        SinkRegistry.Register(captureSink);

        Log.Error("sync-bypass-test");

        SinkRegistry.Remove(captureSink);
        Assert.Equal(callingThreadId, dispatchedThreadId);
    }

    [Fact]
    public void Info_WithDrainStarted_DispatchesOnDrainThread()
    {
        int callingThreadId = Thread.CurrentThread.ManagedThreadId;
        ManualResetEventSlim dispatched = new ManualResetEventSlim(false);
        int dispatchedThreadId = -1;
        ThreadCaptureSink captureSink = new ThreadCaptureSink(id =>
        {
            dispatchedThreadId = id;
            dispatched.Set();
        });
        SinkRegistry.Register(captureSink);

        Logging.EnsureStarted();
        Log.Info("async-drain-test");

        bool signaled = dispatched.Wait(5000);
        SinkRegistry.Remove(captureSink);
        Assert.True(signaled, "Drain thread did not dispatch within 5 s");
        Assert.NotEqual(callingThreadId, dispatchedThreadId);
    }
}

