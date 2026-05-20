using System.Collections.Generic;
using Cryptiklemur.RimLogging;
using Cryptiklemur.RimLogging.Capture;
using Cryptiklemur.RimLogging.Pipeline;
using Xunit;

namespace Cryptiklemur.RimLogging.Tests.Pipeline;

public sealed class ShutdownFlushTests : System.IDisposable
{
    private readonly bool _priorInstalled;

    public ShutdownFlushTests()
    {
        _priorInstalled = ShutdownFlushLogic.Installed;
        ShutdownFlushLogic.ResetInstalledForTests();
    }

    public void Dispose()
    {
        if (_priorInstalled)
            ShutdownFlushLogic.Installed = true;
        else
            ShutdownFlushLogic.ResetInstalledForTests();
    }

    private static LogEntry MakeEntry(int seq) => new LogEntry(
        System.DateTime.UtcNow,
        LogLevel.Info,
        "test",
        $"msg {seq}",
        $"msg {seq}",
        null,
        SourceLocation.Empty,
        null,
        null);

    [Fact]
    public void Flush_WaitsForDrainThenWouldCallFlushAll()
    {
        MpscQueue<LogEntry> queue = new MpscQueue<LogEntry>(64);
        List<LogEntry> dispatched = new List<LogEntry>();

        BackgroundDrain drain = new BackgroundDrain(queue, entry =>
        {
            lock (dispatched)
                dispatched.Add(entry);
        });

        try
        {
            for (int i = 0; i < 5; i++)
                drain.Enqueue(MakeEntry(i));

            ShutdownFlushLogic.Flush(drain);

            Assert.Equal(5, dispatched.Count);
        }
        finally
        {
            drain.Dispose();
        }
    }

    [Fact]
    public void Installed_IsIdempotentGuard()
    {
        ShutdownFlushLogic.ResetInstalledForTests();
        Assert.False(ShutdownFlushLogic.Installed);

        ShutdownFlushLogic.Installed = true;
        Assert.True(ShutdownFlushLogic.Installed);

        ShutdownFlushLogic.ResetInstalledForTests();
        Assert.False(ShutdownFlushLogic.Installed);
    }

    [Fact]
    public void DrainTimeoutMs_Is500()
    {
        Assert.Equal(500, ShutdownFlushLogic.DrainTimeoutMs);
    }
}
