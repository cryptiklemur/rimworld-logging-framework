using System;
using Cryptiklemur.RimLogging.Capture;
using Cryptiklemur.RimLogging.Filtering;
using Xunit;

namespace Cryptiklemur.RimLogging.Tests.Filtering;

public class FilterExpressionRoundTripTests
{
    private static LogEntry MakeEntry(LogLevel lvl, string ch) => new LogEntry(
        timestamp: DateTime.UtcNow,
        level: lvl,
        channel: ch,
        messageTemplate: "",
        renderedMessage: "",
        context: null,
        source: SourceLocation.Empty,
        stackTrace: null,
        exception: null);

    // 1. Parse("level >= Warn").Match — info=false, error=true
    [Fact]
    public void LevelGteWarn_MatchInfoFalse_MatchErrorTrue()
    {
        FilterExpression fe = FilterExpression.Parse("level >= Warn");

        Assert.False(fe.Match(MakeEntry(LogLevel.Info, "default")));
        Assert.True(fe.Match(MakeEntry(LogLevel.Error, "default")));
    }

    // 2. channel = "Cosmere.*" — matches sub-channel, not Unity
    [Fact]
    public void ChannelPattern_MatchSubChannel_NotUnity()
    {
        FilterExpression fe = FilterExpression.Parse("channel = \"Cosmere.*\"");

        Assert.True(fe.Match(MakeEntry(LogLevel.Info, "Cosmere.Roshar")));
        Assert.False(fe.Match(MakeEntry(LogLevel.Info, "Unity")));
    }

    // 3. Source property returns the original input string verbatim
    [Fact]
    public void Source_ReturnsOriginalInput()
    {
        string input = "level >= Warn";
        FilterExpression fe = FilterExpression.Parse(input);

        Assert.Equal(input, fe.Source);
    }

    // 4. ToString returns canonical form
    [Fact]
    public void ToString_LevelCompare_NoExtraParens()
    {
        FilterExpression fe = FilterExpression.Parse("level >= Warn");

        Assert.Equal("level >= Warn", fe.ToString());
    }

    [Fact]
    public void ToString_OrExpression_HasOuterParens()
    {
        FilterExpression fe = FilterExpression.Parse("level >= Warn OR channel = \"X\"");

        Assert.Equal("(level >= Warn OR channel = \"X\")", fe.ToString());
    }

    // 5. Round-trip: Parse(ToString()) evaluates identically
    [Fact]
    public void RoundTrip_ParseStringifyParse_IdenticalEvaluation()
    {
        FilterExpression original = FilterExpression.Parse("level >= Warn OR channel = \"X\"");
        FilterExpression reparsed = FilterExpression.Parse(original.ToString());

        LogEntry[] entries =
        [
            MakeEntry(LogLevel.Trace, "X"),
            MakeEntry(LogLevel.Info,  "X"),
            MakeEntry(LogLevel.Warn,  "default"),
            MakeEntry(LogLevel.Error, "other"),
            MakeEntry(LogLevel.Info,  "other"),
        ];

        foreach (LogEntry entry in entries)
            Assert.Equal(original.Match(entry), reparsed.Match(entry));
    }
}
