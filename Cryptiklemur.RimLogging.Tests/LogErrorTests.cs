using System;
using System.Collections.Generic;
using Cryptiklemur.RimLogging;
using Xunit;

namespace Cryptiklemur.RimLogging.Tests
{
    public class LogErrorTests : IDisposable
    {
        private readonly LogLevel _savedMin;
        private readonly Action<LogEntry>? _savedOverride;
        private readonly List<LogEntry> _captured = new List<LogEntry>();

        public LogErrorTests()
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
        public void Error_DefaultChannelTemplate_RoutesAtCorrectLevel()
        {
            Log.Error("error-level-test-sentinel");

            LogEntry? entry = _captured.Count > 0 ? _captured[_captured.Count - 1] : null;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Error, entry!.Level);
            Assert.Equal("default", entry.Channel);
        }

        [Fact]
        public void Error_Exception_DefaultChannel_PopulatesEntryException()
        {
            Exception ex = new InvalidOperationException("error-ex-test");

            Log.Error(ex, "error-exception-message");

            LogEntry? entry = _captured.Count > 0 ? _captured[_captured.Count - 1] : null;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Error, entry!.Level);
            Assert.Same(ex, entry.Exception);
        }

        [Fact]
        public void Error_Exception_ExplicitChannel_PopulatesEntryException()
        {
            Exception ex = new InvalidOperationException("error-ex-channel-test");

            Log.Error("error-chan", ex, "error-exception-channel-message");

            LogEntry? entry = _captured.Count > 0 ? _captured[_captured.Count - 1] : null;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Error, entry!.Level);
            Assert.Equal("error-chan", entry.Channel);
            Assert.Same(ex, entry.Exception);
        }

        [Fact]
        public void Error_BelowGlobalMinLevel_IsDropped()
        {
            Logging.GlobalMinLevel = LogLevel.Fatal;
            int countBefore = _captured.Count;

            Log.Error("dropped-error-sentinel");

            Assert.Equal(countBefore, _captured.Count);
        }

        [Fact]
        public void Error_ExplicitChannel_RoutesChannelUnchanged()
        {
            Log.Error("error-audit", "explicit-channel-error-sentinel");

            LogEntry? entry = _captured.Count > 0 ? _captured[_captured.Count - 1] : null;
            Assert.NotNull(entry);
            Assert.Equal("error-audit", entry!.Channel);
            Assert.Equal(LogLevel.Error, entry.Level);
        }
    }
}
