using System;
using Cryptiklemur.RimLogging;
using Cryptiklemur.RimLogging.Sinks;
using Xunit;

namespace Cryptiklemur.RimLogging.Tests
{
    public class LogErrorTests : IDisposable
    {
        private readonly LogLevel _savedMin;
        private readonly MemoryLogSink _sink = new MemoryLogSink();

        public LogErrorTests()
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
        public void Error_DefaultChannelTemplate_RoutesAtCorrectLevel()
        {
            Log.Error("error-level-test-sentinel");

            LogEntry? entry = _sink.Entries.Count > 0 ? _sink.Entries[_sink.Entries.Count - 1] : null;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Error, entry!.Level);
            Assert.Equal("default", entry.Channel);
        }

        [Fact]
        public void Error_Exception_DefaultChannel_PopulatesEntryException()
        {
            Exception ex = new InvalidOperationException("error-ex-test");

            Log.Error(ex, "error-exception-message");

            LogEntry? entry = _sink.Entries.Count > 0 ? _sink.Entries[_sink.Entries.Count - 1] : null;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Error, entry!.Level);
            Assert.Same(ex, entry.Exception);
        }

        [Fact]
        public void Error_Exception_ExplicitChannel_PopulatesEntryException()
        {
            Exception ex = new InvalidOperationException("error-ex-channel-test");

            Log.Error("error-chan", ex, "error-exception-channel-message");

            LogEntry? entry = _sink.Entries.Count > 0 ? _sink.Entries[_sink.Entries.Count - 1] : null;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Error, entry!.Level);
            Assert.Equal("error-chan", entry.Channel);
            Assert.Same(ex, entry.Exception);
        }

        [Fact]
        public void Error_BelowGlobalMinLevel_IsDropped()
        {
            Logging.GlobalMinLevel = LogLevel.Fatal;
            int countBefore = _sink.Entries.Count;

            Log.Error("dropped-error-sentinel");

            Assert.Equal(countBefore, _sink.Entries.Count);
        }

        [Fact]
        public void Error_ExplicitChannel_RoutesChannelUnchanged()
        {
            Log.Error("error-audit", "explicit-channel-error-sentinel");

            LogEntry? entry = _sink.Entries.Count > 0 ? _sink.Entries[_sink.Entries.Count - 1] : null;
            Assert.NotNull(entry);
            Assert.Equal("error-audit", entry!.Channel);
            Assert.Equal(LogLevel.Error, entry.Level);
        }
    }
}
