using System;
using System.Collections.Generic;
using Cryptiklemur.RimLogging;
using Xunit;

namespace Cryptiklemur.RimLogging.Tests
{
    public class LogDebugTests : IDisposable
    {
        private readonly LogLevel _savedMin;
        private readonly Action<LogEntry>? _savedOverride;
        private readonly List<LogEntry> _captured = new List<LogEntry>();

        public LogDebugTests()
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
        public void Debug_DefaultChannelTemplate_RoutesAtCorrectLevel()
        {
            Log.Debug("debug-level-test-sentinel");

            LogEntry? entry = _captured.Count > 0 ? _captured[_captured.Count - 1] : null;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Debug, entry!.Level);
            Assert.Equal("default", entry.Channel);
        }

        [Fact]
        public void Debug_Exception_DefaultChannel_PopulatesEntryException()
        {
            Exception ex = new InvalidOperationException("debug-ex-test");

            Log.Debug(ex, "debug-exception-message");

            LogEntry? entry = _captured.Count > 0 ? _captured[_captured.Count - 1] : null;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Debug, entry!.Level);
            Assert.Same(ex, entry.Exception);
        }

        [Fact]
        public void Debug_Exception_ExplicitChannel_PopulatesEntryException()
        {
            Exception ex = new InvalidOperationException("debug-ex-channel-test");

            Log.Debug("debug-chan", ex, "debug-exception-channel-message");

            LogEntry? entry = _captured.Count > 0 ? _captured[_captured.Count - 1] : null;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Debug, entry!.Level);
            Assert.Equal("debug-chan", entry.Channel);
            Assert.Same(ex, entry.Exception);
        }

        [Fact]
        public void Debug_BelowGlobalMinLevel_IsDropped()
        {
            Logging.GlobalMinLevel = LogLevel.Info;
            int countBefore = _captured.Count;

            Log.Debug("dropped-debug-sentinel");

            Assert.Equal(countBefore, _captured.Count);
        }

        [Fact]
        public void Debug_ExplicitChannel_RoutesChannelUnchanged()
        {
            Log.Debug("debug-audit", "explicit-channel-debug-sentinel");

            LogEntry? entry = _captured.Count > 0 ? _captured[_captured.Count - 1] : null;
            Assert.NotNull(entry);
            Assert.Equal("debug-audit", entry!.Channel);
            Assert.Equal(LogLevel.Debug, entry.Level);
        }
    }
}
