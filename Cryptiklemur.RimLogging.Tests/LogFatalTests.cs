using System;
using Cryptiklemur.RimLogging;
using Cryptiklemur.RimLogging.Sinks;
using Xunit;

namespace Cryptiklemur.RimLogging.Tests
{
    public class LogFatalTests : IDisposable
    {
        private readonly LogLevel _savedMin;
        private readonly MemoryLogSink _sink = new MemoryLogSink();

        public LogFatalTests()
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
        public void Fatal_DefaultChannelTemplate_RoutesAtCorrectLevel()
        {
            Log.Fatal("fatal-level-test-sentinel");

            LogEntry? entry = _sink.Entries.Count > 0 ? _sink.Entries[_sink.Entries.Count - 1] : null;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Fatal, entry!.Level);
            Assert.Equal("default", entry.Channel);
        }

        [Fact]
        public void Fatal_Exception_DefaultChannel_PopulatesEntryException()
        {
            Exception ex = new InvalidOperationException("fatal-ex-test");

            Log.Fatal(ex, "fatal-exception-message");

            LogEntry? entry = _sink.Entries.Count > 0 ? _sink.Entries[_sink.Entries.Count - 1] : null;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Fatal, entry!.Level);
            Assert.Same(ex, entry.Exception);
        }

        [Fact]
        public void Fatal_Exception_ExplicitChannel_PopulatesEntryException()
        {
            Exception ex = new InvalidOperationException("fatal-ex-channel-test");

            Log.Fatal("fatal-chan", ex, "fatal-exception-channel-message");

            LogEntry? entry = _sink.Entries.Count > 0 ? _sink.Entries[_sink.Entries.Count - 1] : null;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Fatal, entry!.Level);
            Assert.Equal("fatal-chan", entry.Channel);
            Assert.Same(ex, entry.Exception);
        }

        [Fact]
        public void Fatal_ExplicitChannel_RoutesChannelUnchanged()
        {
            Log.Fatal("fatal-audit", "explicit-channel-fatal-sentinel");

            LogEntry? entry = _sink.Entries.Count > 0 ? _sink.Entries[_sink.Entries.Count - 1] : null;
            Assert.NotNull(entry);
            Assert.Equal("fatal-audit", entry!.Channel);
            Assert.Equal(LogLevel.Fatal, entry.Level);
        }
    }
}
