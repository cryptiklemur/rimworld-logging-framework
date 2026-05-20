using System;
using Cryptiklemur.RimLogging;
using Xunit;

namespace Cryptiklemur.RimLogging.Tests
{
    public class LogTraceTests : IDisposable
    {
        private readonly LogLevel _savedMin;
        private readonly LogEntry? _savedLast;
        private readonly int _savedCount;

        public LogTraceTests()
        {
            _savedMin = Logging.GlobalMinLevel;
            _savedLast = Logging.LastEntry;
            _savedCount = Logging.EntriesSeen;
            Logging.GlobalMinLevel = LogLevel.Trace;
            Logging.LastEntry = null;
            Logging.EntriesSeen = 0;
        }

        public void Dispose()
        {
            Logging.GlobalMinLevel = _savedMin;
            Logging.LastEntry = _savedLast;
            Logging.EntriesSeen = _savedCount;
        }

        [Fact]
        public void Trace_DefaultChannelTemplate_RoutesAtCorrectLevel()
        {
            Log.Trace("trace-level-test-sentinel");

            LogEntry? entry = Logging.LastEntry;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Trace, entry!.Level);
            Assert.Equal("default", entry.Channel);
        }

        [Fact]
        public void Trace_Exception_DefaultChannel_PopulatesEntryException()
        {
            Exception ex = new InvalidOperationException("trace-ex-test");

            Log.Trace(ex, "trace-exception-message");

            LogEntry? entry = Logging.LastEntry;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Trace, entry!.Level);
            Assert.Same(ex, entry.Exception);
        }

        [Fact]
        public void Trace_Exception_ExplicitChannel_PopulatesEntryException()
        {
            Exception ex = new InvalidOperationException("trace-ex-channel-test");

            Log.Trace("trace-chan", ex, "trace-exception-channel-message");

            LogEntry? entry = Logging.LastEntry;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Trace, entry!.Level);
            Assert.Equal("trace-chan", entry.Channel);
            Assert.Same(ex, entry.Exception);
        }

        [Fact]
        public void Trace_BelowGlobalMinLevel_IsDropped()
        {
            Logging.GlobalMinLevel = LogLevel.Debug;
            int countBefore = Logging.EntriesSeen;

            Log.Trace("dropped-trace-sentinel");

            Assert.Equal(countBefore, Logging.EntriesSeen);
        }

        [Fact]
        public void Trace_ExplicitChannel_RoutesChannelUnchanged()
        {
            Log.Trace("trace-audit", "explicit-channel-trace-sentinel");

            LogEntry? entry = Logging.LastEntry;
            Assert.NotNull(entry);
            Assert.Equal("trace-audit", entry!.Channel);
            Assert.Equal(LogLevel.Trace, entry.Level);
        }
    }
}
