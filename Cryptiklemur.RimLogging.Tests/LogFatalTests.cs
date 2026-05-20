using System;
using Cryptiklemur.RimLogging;
using Xunit;

namespace Cryptiklemur.RimLogging.Tests
{
    public class LogFatalTests : IDisposable
    {
        private readonly LogLevel _savedMin;
        private readonly LogEntry? _savedLast;
        private readonly int _savedCount;

        public LogFatalTests()
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
        public void Fatal_DefaultChannelTemplate_RoutesAtCorrectLevel()
        {
            Log.Fatal("fatal-level-test-sentinel");

            LogEntry? entry = Logging.LastEntry;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Fatal, entry!.Level);
            Assert.Equal("default", entry.Channel);
        }

        [Fact]
        public void Fatal_Exception_DefaultChannel_PopulatesEntryException()
        {
            Exception ex = new InvalidOperationException("fatal-ex-test");

            Log.Fatal(ex, "fatal-exception-message");

            LogEntry? entry = Logging.LastEntry;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Fatal, entry!.Level);
            Assert.Same(ex, entry.Exception);
        }

        [Fact]
        public void Fatal_Exception_ExplicitChannel_PopulatesEntryException()
        {
            Exception ex = new InvalidOperationException("fatal-ex-channel-test");

            Log.Fatal("fatal-chan", ex, "fatal-exception-channel-message");

            LogEntry? entry = Logging.LastEntry;
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Fatal, entry!.Level);
            Assert.Equal("fatal-chan", entry.Channel);
            Assert.Same(ex, entry.Exception);
        }

        [Fact]
        public void Fatal_ExplicitChannel_RoutesChannelUnchanged()
        {
            Log.Fatal("fatal-audit", "explicit-channel-fatal-sentinel");

            LogEntry? entry = Logging.LastEntry;
            Assert.NotNull(entry);
            Assert.Equal("fatal-audit", entry!.Channel);
            Assert.Equal(LogLevel.Fatal, entry.Level);
        }
    }
}
