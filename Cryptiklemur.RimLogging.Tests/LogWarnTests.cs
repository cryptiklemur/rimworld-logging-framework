using System;
using System.Collections.Generic;
using Cryptiklemur.RimLogging;
using Xunit;

namespace Cryptiklemur.RimLogging.Tests
{
    public class LogWarnTests : IDisposable
    {
        private readonly LogLevel _savedMin;
        private readonly Action<LogEntry>? _savedOverride;
        private readonly List<LogEntry> _captured = new List<LogEntry>();

        public LogWarnTests()
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
        public void Warn_DefaultChannelTemplate_RoutesAtCorrectLevel()
        {
            Log.Warn("warn-level-test-sentinel");

            LogEntry? entry = _captured.Count > 0 ? _captured[_captured.Count - 1] : null;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Warn, entry!.Level);
            Assert.Equal("default", entry.Channel);
        }

        [Fact]
        public void Warn_Exception_DefaultChannel_PopulatesEntryException()
        {
            Exception ex = new InvalidOperationException("warn-ex-test");

            Log.Warn(ex, "warn-exception-message");

            LogEntry? entry = _captured.Count > 0 ? _captured[_captured.Count - 1] : null;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Warn, entry!.Level);
            Assert.Same(ex, entry.Exception);
        }

        [Fact]
        public void Warn_Exception_ExplicitChannel_PopulatesEntryException()
        {
            Exception ex = new InvalidOperationException("warn-ex-channel-test");

            Log.Warn("warn-chan", ex, "warn-exception-channel-message");

            LogEntry? entry = _captured.Count > 0 ? _captured[_captured.Count - 1] : null;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Warn, entry!.Level);
            Assert.Equal("warn-chan", entry.Channel);
            Assert.Same(ex, entry.Exception);
        }

        [Fact]
        public void Warn_BelowGlobalMinLevel_IsDropped()
        {
            Logging.GlobalMinLevel = LogLevel.Error;
            int countBefore = _captured.Count;

            Log.Warn("dropped-warn-sentinel");

            Assert.Equal(countBefore, _captured.Count);
        }

        [Fact]
        public void Warn_ExplicitChannel_RoutesChannelUnchanged()
        {
            Log.Warn("warn-audit", "explicit-channel-warn-sentinel");

            LogEntry? entry = _captured.Count > 0 ? _captured[_captured.Count - 1] : null;
            Assert.NotNull(entry);
            Assert.Equal("warn-audit", entry!.Channel);
            Assert.Equal(LogLevel.Warn, entry.Level);
        }
    }
}
