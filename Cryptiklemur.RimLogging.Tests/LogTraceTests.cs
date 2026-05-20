using System;
using System.Collections.Generic;
using Cryptiklemur.RimLogging;
using Xunit;

namespace Cryptiklemur.RimLogging.Tests
{
    public class LogTraceTests : IDisposable
    {
        private readonly LogLevel _savedMin;
        private readonly Action<LogEntry>? _savedOverride;
        private readonly List<LogEntry> _captured = new List<LogEntry>();

        public LogTraceTests()
        {
            _savedMin = Logging.GlobalMinLevel;
            _savedOverride = Logging._dispatchSyncOverride;
            Logging.GlobalMinLevel = LogLevel.Trace;
            Logging._dispatchSyncOverride = e => _captured.Add(e);
        }

        public void Dispose()
        {
            Logging.GlobalMinLevel = _savedMin;
            Logging._dispatchSyncOverride = _savedOverride;
        }

        [Fact]
        public void Trace_DefaultChannelTemplate_RoutesAtCorrectLevel()
        {
            Log.Trace("trace-level-test-sentinel");

            LogEntry? entry = _captured.Count > 0 ? _captured[_captured.Count - 1] : null;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Trace, entry!.Level);
            Assert.Equal("default", entry.Channel);
        }

        [Fact]
        public void Trace_Exception_DefaultChannel_PopulatesEntryException()
        {
            Exception ex = new InvalidOperationException("trace-ex-test");

            Log.Trace(ex, "trace-exception-message");

            LogEntry? entry = _captured.Count > 0 ? _captured[_captured.Count - 1] : null;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Trace, entry!.Level);
            Assert.Same(ex, entry.Exception);
        }

        [Fact]
        public void Trace_Exception_ExplicitChannel_PopulatesEntryException()
        {
            Exception ex = new InvalidOperationException("trace-ex-channel-test");

            Log.Trace("trace-chan", ex, "trace-exception-channel-message");

            LogEntry? entry = _captured.Count > 0 ? _captured[_captured.Count - 1] : null;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Trace, entry!.Level);
            Assert.Equal("trace-chan", entry.Channel);
            Assert.Same(ex, entry.Exception);
        }

        [Fact]
        public void Trace_BelowGlobalMinLevel_IsDropped()
        {
            Logging.GlobalMinLevel = LogLevel.Debug;
            int countBefore = _captured.Count;

            Log.Trace("dropped-trace-sentinel");

            Assert.Equal(countBefore, _captured.Count);
        }

        [Fact]
        public void Trace_ExplicitChannel_RoutesChannelUnchanged()
        {
            Log.Trace("trace-audit", "explicit-channel-trace-sentinel");

            LogEntry? entry = _captured.Count > 0 ? _captured[_captured.Count - 1] : null;
            Assert.NotNull(entry);
            Assert.Equal("trace-audit", entry!.Channel);
            Assert.Equal(LogLevel.Trace, entry.Level);
        }
    }
}
