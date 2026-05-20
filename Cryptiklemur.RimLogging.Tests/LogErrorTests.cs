using System;
using Cryptiklemur.RimLogging;
using Xunit;

namespace Cryptiklemur.RimLogging.Tests
{
    public class LogErrorTests : IDisposable
    {
        private readonly LogLevel _savedMin;
        private readonly LogEntry? _savedLast;
        private readonly int _savedCount;

        public LogErrorTests()
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
        public void Error_DefaultChannelTemplate_RoutesAtCorrectLevel()
        {
            Log.Error("error-level-test-sentinel");

            LogEntry? entry = Logging.LastEntry;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Error, entry!.Level);
            Assert.Equal("default", entry.Channel);
        }

        [Fact]
        public void Error_Exception_DefaultChannel_PopulatesEntryException()
        {
            Exception ex = new InvalidOperationException("error-ex-test");

            Log.Error(ex, "error-exception-message");

            LogEntry? entry = Logging.LastEntry;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Error, entry!.Level);
            Assert.Same(ex, entry.Exception);
        }

        [Fact]
        public void Error_Exception_ExplicitChannel_PopulatesEntryException()
        {
            Exception ex = new InvalidOperationException("error-ex-channel-test");

            Log.Error("error-chan", ex, "error-exception-channel-message");

            LogEntry? entry = Logging.LastEntry;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Error, entry!.Level);
            Assert.Equal("error-chan", entry.Channel);
            Assert.Same(ex, entry.Exception);
        }

        [Fact]
        public void Error_BelowGlobalMinLevel_IsDropped()
        {
            Logging.GlobalMinLevel = LogLevel.Fatal;
            int countBefore = Logging.EntriesSeen;

            Log.Error("dropped-error-sentinel");

            Assert.Equal(countBefore, Logging.EntriesSeen);
        }

        [Fact]
        public void Error_ExplicitChannel_RoutesChannelUnchanged()
        {
            Log.Error("error-audit", "explicit-channel-error-sentinel");

            LogEntry? entry = Logging.LastEntry;
            Assert.NotNull(entry);
            Assert.Equal("error-audit", entry!.Channel);
            Assert.Equal(LogLevel.Error, entry.Level);
        }
    }
}
