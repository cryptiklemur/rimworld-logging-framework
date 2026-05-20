using System;
using Cryptiklemur.RimLogging;
using Xunit;

namespace Cryptiklemur.RimLogging.Tests
{
    public class LogWarnTests : IDisposable
    {
        private readonly LogLevel _savedMin;
        private readonly LogEntry? _savedLast;
        private readonly int _savedCount;

        public LogWarnTests()
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
        public void Warn_DefaultChannelTemplate_RoutesAtCorrectLevel()
        {
            Log.Warn("warn-level-test-sentinel");

            LogEntry? entry = Logging.LastEntry;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Warn, entry!.Level);
            Assert.Equal("default", entry.Channel);
        }

        [Fact]
        public void Warn_Exception_DefaultChannel_PopulatesEntryException()
        {
            Exception ex = new InvalidOperationException("warn-ex-test");

            Log.Warn(ex, "warn-exception-message");

            LogEntry? entry = Logging.LastEntry;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Warn, entry!.Level);
            Assert.Same(ex, entry.Exception);
        }

        [Fact]
        public void Warn_Exception_ExplicitChannel_PopulatesEntryException()
        {
            Exception ex = new InvalidOperationException("warn-ex-channel-test");

            Log.Warn("warn-chan", ex, "warn-exception-channel-message");

            LogEntry? entry = Logging.LastEntry;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Warn, entry!.Level);
            Assert.Equal("warn-chan", entry.Channel);
            Assert.Same(ex, entry.Exception);
        }

        [Fact]
        public void Warn_BelowGlobalMinLevel_IsDropped()
        {
            Logging.GlobalMinLevel = LogLevel.Error;
            int countBefore = Logging.EntriesSeen;

            Log.Warn("dropped-warn-sentinel");

            Assert.Equal(countBefore, Logging.EntriesSeen);
        }

        [Fact]
        public void Warn_ExplicitChannel_RoutesChannelUnchanged()
        {
            Log.Warn("warn-audit", "explicit-channel-warn-sentinel");

            LogEntry? entry = Logging.LastEntry;
            Assert.NotNull(entry);
            Assert.Equal("warn-audit", entry!.Channel);
            Assert.Equal(LogLevel.Warn, entry.Level);
        }
    }
}
