using System;
using System.Collections.Generic;
using Cryptiklemur.RimLogging;
using Cryptiklemur.RimLogging.Capture;
using Xunit;

namespace Cryptiklemur.RimLogging.Tests;

public class LogEntryTests
{
    [Fact]
    public void Construct_PopulatesAllFields()
    {
        DateTime ts = new DateTime(2026, 5, 20, 14, 23, 11, 234, DateTimeKind.Utc);
        SourceLocation src = new SourceLocation("Player.cs", 42, "Spawn");
        Dictionary<string, object?> ctx = new Dictionary<string, object?> { ["Hp"] = 5 };

        LogEntry e = new LogEntry(
            timestamp: ts,
            level: LogLevel.Info,
            channel: "Cosmere.Roshar",
            messageTemplate: "died at {Hp}hp",
            renderedMessage: "died at 5hp",
            context: ctx,
            source: src,
            stackTrace: null,
            exception: null);

        Assert.Equal(ts, e.Timestamp);
        Assert.Equal(LogLevel.Info, e.Level);
        Assert.Equal("Cosmere.Roshar", e.Channel);
        Assert.Equal("died at {Hp}hp", e.MessageTemplate);
        Assert.Equal("died at 5hp", e.RenderedMessage);
        Assert.Same(ctx, e.Context);
        Assert.Equal(src, e.Source);
        Assert.Null(e.StackTrace);
        Assert.Null(e.Exception);
    }

    [Fact]
    public void Construct_ThrowsOnNullChannel()
    {
        SourceLocation src = new SourceLocation("Player.cs", 1, "Init");

        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            new LogEntry(
                timestamp: DateTime.UtcNow,
                level: LogLevel.Info,
                channel: null!,
                messageTemplate: "msg",
                renderedMessage: "msg",
                context: null,
                source: src,
                stackTrace: null,
                exception: null));

        Assert.Equal("channel", ex.ParamName);
    }

    [Fact]
    public void Construct_CoalescesNullTemplatesToEmpty()
    {
        SourceLocation src = new SourceLocation("Player.cs", 1, "Init");

        LogEntry e = new LogEntry(
            timestamp: DateTime.UtcNow,
            level: LogLevel.Info,
            channel: "Test",
            messageTemplate: null!,
            renderedMessage: null!,
            context: null,
            source: src,
            stackTrace: null,
            exception: null);

        Assert.Equal(string.Empty, e.MessageTemplate);
        Assert.Equal(string.Empty, e.RenderedMessage);
    }
}
