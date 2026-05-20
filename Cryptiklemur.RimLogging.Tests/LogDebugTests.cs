using System;
using Cryptiklemur.RimLogging;
using Xunit;

namespace Cryptiklemur.RimLogging.Tests
{
    public class LogDebugTests : IDisposable
    {
        private readonly LogLevel _savedMin;
        private readonly LogEntry? _savedLast;
        private readonly int _savedCount;

        public LogDebugTests()
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
        public void Debug_DefaultChannelTemplate_RoutesAtCorrectLevel()
        {
            Log.Debug("debug-level-test-sentinel");

            LogEntry? entry = Logging.LastEntry;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Debug, entry!.Level);
            Assert.Equal("default", entry.Channel);
        }

        [Fact]
        public void Debug_Exception_DefaultChannel_PopulatesEntryException()
        {
            Exception ex = new InvalidOperationException("debug-ex-test");

            Log.Debug(ex, "debug-exception-message");

            LogEntry? entry = Logging.LastEntry;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Debug, entry!.Level);
            Assert.Same(ex, entry.Exception);
        }

        [Fact]
        public void Debug_Exception_ExplicitChannel_PopulatesEntryException()
        {
            Exception ex = new InvalidOperationException("debug-ex-channel-test");

            Log.Debug("debug-chan", ex, "debug-exception-channel-message");

            LogEntry? entry = Logging.LastEntry;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Debug, entry!.Level);
            Assert.Equal("debug-chan", entry.Channel);
            Assert.Same(ex, entry.Exception);
        }

        [Fact]
        public void Debug_BelowGlobalMinLevel_IsDropped()
        {
            Logging.GlobalMinLevel = LogLevel.Info;
            int countBefore = Logging.EntriesSeen;

            Log.Debug("dropped-debug-sentinel");

            Assert.Equal(countBefore, Logging.EntriesSeen);
        }

        [Fact]
        public void Debug_ExplicitChannel_RoutesChannelUnchanged()
        {
            Log.Debug("debug-audit", "explicit-channel-debug-sentinel");

            LogEntry? entry = Logging.LastEntry;
            Assert.NotNull(entry);
            Assert.Equal("debug-audit", entry!.Channel);
            Assert.Equal(LogLevel.Debug, entry.Level);
        }
    }
}
