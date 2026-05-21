using System;
using Cryptiklemur.RimLogging;
using Cryptiklemur.RimLogging.Capture;
using Cryptiklemur.RimLogging.Sinks;
using Xunit;

namespace Cryptiklemur.RimLogging.Tests.Pipeline;

public class EmitInternalForUnityTests : IDisposable
{
    private readonly LogLevel _savedMin;
    private readonly MemoryLogSink _sink = new MemoryLogSink();

    public EmitInternalForUnityTests()
    {
        _savedMin = Logging.GlobalMinLevel;
        SinkRegistry.DisposeAll();
        SinkRegistry.Register(_sink);
        Logging.GlobalMinLevel = LogLevel.Trace;
    }

    public void Dispose()
    {
        Logging.GlobalMinLevel = _savedMin;
        SinkRegistry.Remove(_sink);
        _sink.Dispose();
    }

    [Fact]
    public void EmitInternalForUnity_PopulatesEntry()
    {
        Log.EmitInternalForUnity(LogLevel.Error, "Unity", "boom", "stack here");

        LogEntry entry = Assert.Single(_sink.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Equal("Unity", entry.Channel);
        Assert.Equal("boom", entry.RenderedMessage);
        Assert.Equal("boom", entry.MessageTemplate);
        Assert.Equal("stack here", entry.StackTrace);
        Assert.Null(entry.Exception);
        Assert.False(entry.Source.IsCallerProvided);
    }

    [Fact]
    public void EmitInternalForUnity_BelowGlobalMinLevel_IsDropped()
    {
        Logging.GlobalMinLevel = LogLevel.Warn;

        Log.EmitInternalForUnity(LogLevel.Info, "Unity", "ignored", "stack");

        Assert.Empty(_sink.Entries);
    }

    [Fact]
    public void EmitInternalForUnity_NullCondition_NormalisesToEmpty()
    {
        Log.EmitInternalForUnity(LogLevel.Error, "Unity", null!, "stack");

        LogEntry entry = Assert.Single(_sink.Entries);
        Assert.Equal(string.Empty, entry.RenderedMessage);
        Assert.Equal(string.Empty, entry.MessageTemplate);
    }
}
