using System;
using System.Collections.Generic;
using Cryptiklemur.RimLogging;
using Cryptiklemur.RimLogging.Sinks;
using Xunit;

namespace Cryptiklemur.RimLogging.Tests.Sinks;

public class SinkRegistryTests : IDisposable
{
    public SinkRegistryTests()
    {
        SinkRegistry.DisposeAll();
    }

    public void Dispose()
    {
        SinkRegistry.DisposeAll();
    }

    private static LogEntry MakeEntry(string message = "test")
    {
        return new LogEntry(
            timestamp: DateTime.UtcNow,
            level: LogLevel.Info,
            channel: "test",
            messageTemplate: message,
            renderedMessage: message,
            context: null,
            source: default,
            stackTrace: null,
            exception: null);
    }

    [Fact]
    public void Register_AddsSinkToSnapshot()
    {
        FakeSink sink = new FakeSink("alpha");

        SinkRegistry.Register(sink);

        IReadOnlyList<ILogSink> snap = SinkRegistry.Snapshot();
        Assert.Contains(sink, snap);
    }

    [Fact]
    public void Remove_ExistingSink_ReturnsTrueAndRemovesFromSnapshot()
    {
        FakeSink sink = new FakeSink("beta");
        SinkRegistry.Register(sink);

        bool removed = SinkRegistry.Remove(sink);

        Assert.True(removed);
        Assert.DoesNotContain(sink, SinkRegistry.Snapshot());
    }

    [Fact]
    public void Remove_AbsentSink_ReturnsFalse()
    {
        FakeSink sink = new FakeSink("gamma");

        bool removed = SinkRegistry.Remove(sink);

        Assert.False(removed);
    }

    [Fact]
    public void Snapshot_ReturnsStableCopy_NotAffectedByLaterMutation()
    {
        FakeSink first = new FakeSink("delta-1");
        SinkRegistry.Register(first);

        IReadOnlyList<ILogSink> snap = SinkRegistry.Snapshot();

        FakeSink second = new FakeSink("delta-2");
        SinkRegistry.Register(second);

        Assert.DoesNotContain(second, snap);
    }

    [Fact]
    public void DispatchSynchronously_WritesToAllRegisteredSinks()
    {
        FakeSink a = new FakeSink("epsilon-a");
        FakeSink b = new FakeSink("epsilon-b");
        SinkRegistry.Register(a);
        SinkRegistry.Register(b);
        LogEntry entry = MakeEntry("dispatch-test");

        SinkRegistry.DispatchSynchronously(entry);

        Assert.Equal(1, a.WriteCount);
        Assert.Equal(1, b.WriteCount);
        Assert.Same(entry, a.LastWritten);
        Assert.Same(entry, b.LastWritten);
    }

    [Fact]
    public void DispatchSynchronously_SuppressesReentry_RecursiveCallIsIgnored()
    {
        LogEntry entry = MakeEntry("reentry-test");
        ReentrantSink reentrant = new ReentrantSink("zeta", entry);
        SinkRegistry.Register(reentrant);

        SinkRegistry.DispatchSynchronously(entry);

        // The sink was called once; its internal recursive call was suppressed.
        Assert.Equal(1, reentrant.WriteCount);
    }

    [Fact]
    public void DispatchSynchronously_ThrowingSink_DoesNotPreventOtherSinksFromReceivingEntry()
    {
        ThrowingSink bad = new ThrowingSink("eta-bad");
        FakeSink good = new FakeSink("eta-good");
        SinkRegistry.Register(bad);
        SinkRegistry.Register(good);
        LogEntry entry = MakeEntry("throw-test");

        SinkRegistry.DispatchSynchronously(entry);

        Assert.Equal(1, good.WriteCount);
    }

    [Fact]
    public void FlushAll_CallsFlushOnEveryRegisteredSink()
    {
        FakeSink a = new FakeSink("theta-a");
        FakeSink b = new FakeSink("theta-b");
        SinkRegistry.Register(a);
        SinkRegistry.Register(b);

        SinkRegistry.FlushAll();

        Assert.Equal(1, a.FlushCount);
        Assert.Equal(1, b.FlushCount);
    }

    [Fact]
    public void FlushAll_ThrowingSink_DoesNotPreventOtherSinksFromBeingFlushed()
    {
        ThrowingSink bad = new ThrowingSink("iota-bad");
        FakeSink good = new FakeSink("iota-good");
        SinkRegistry.Register(bad);
        SinkRegistry.Register(good);

        SinkRegistry.FlushAll();

        Assert.Equal(1, good.FlushCount);
    }

    [Fact]
    public void DisposeAll_EmptiesRegistryAndDisposesEachSink()
    {
        FakeSink a = new FakeSink("kappa-a");
        FakeSink b = new FakeSink("kappa-b");
        SinkRegistry.Register(a);
        SinkRegistry.Register(b);

        SinkRegistry.DisposeAll();

        Assert.Empty(SinkRegistry.Snapshot());
        Assert.True(a.Disposed);
        Assert.True(b.Disposed);
    }

    [Fact]
    public void DisposeAll_ThrowingSink_DoesNotPreventOtherSinksFromBeingDisposed()
    {
        ThrowingSink bad = new ThrowingSink("lambda-bad");
        FakeSink good = new FakeSink("lambda-good");
        SinkRegistry.Register(bad);
        SinkRegistry.Register(good);

        SinkRegistry.DisposeAll();

        Assert.True(good.Disposed);
    }

    private sealed class FakeSink : ILogSink
    {
        public FakeSink(string name) { Name = name; }

        public string Name { get; }
        public LogLevel MinLevel => LogLevel.Trace;
        public int WriteCount { get; private set; }
        public int FlushCount { get; private set; }
        public bool Disposed { get; private set; }
        public LogEntry? LastWritten { get; private set; }

        public void Write(LogEntry entry)
        {
            WriteCount++;
            LastWritten = entry;
        }

        public void Flush() { FlushCount++; }
        public void Dispose() { Disposed = true; }
    }

    /// <summary>Sink that calls DispatchSynchronously recursively during Write.</summary>
    private sealed class ReentrantSink : ILogSink
    {
        private readonly LogEntry _entryToRedispatch;

        public ReentrantSink(string name, LogEntry entryToRedispatch)
        {
            Name = name;
            _entryToRedispatch = entryToRedispatch;
        }

        public string Name { get; }
        public LogLevel MinLevel => LogLevel.Trace;
        public int WriteCount { get; private set; }

        public void Write(LogEntry entry)
        {
            WriteCount++;
            SinkRegistry.DispatchSynchronously(_entryToRedispatch);
        }

        public void Flush() { }
        public void Dispose() { }
    }

    /// <summary>Sink that throws on every operation.</summary>
    private sealed class ThrowingSink : ILogSink
    {
        public ThrowingSink(string name) { Name = name; }

        public string Name { get; }
        public LogLevel MinLevel => LogLevel.Trace;

        public void Write(LogEntry entry) { throw new InvalidOperationException("sink write failure"); }
        public void Flush() { throw new InvalidOperationException("sink flush failure"); }
        public void Dispose() { throw new InvalidOperationException("sink dispose failure"); }
    }
}
