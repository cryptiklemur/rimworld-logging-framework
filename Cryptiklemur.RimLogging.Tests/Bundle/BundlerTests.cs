using System;
using System.Collections.Generic;
using Cryptiklemur.RimLogging.Bundle;
using Cryptiklemur.RimLogging.Capture;
using Xunit;

namespace Cryptiklemur.RimLogging.Tests.Bundle;

public class BundlerTests
{
    private static LogEntry MakeEntry(
        string renderedMessage = "hello",
        string channel = "default",
        LogLevel level = LogLevel.Info,
        IReadOnlyDictionary<string, object?>? ctx = null,
        Exception? ex = null,
        string? stackTrace = null,
        SourceLocation source = default)
    {
        return new LogEntry(
            timestamp: new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc),
            level: level,
            channel: channel,
            messageTemplate: renderedMessage,
            renderedMessage: renderedMessage,
            context: ctx,
            source: source,
            stackTrace: stackTrace,
            exception: ex);
    }

    [Fact]
    public void Build_EmptyEntries_PopulatesMetadataAndEmptyEntriesList()
    {
        BundlePayload p = Bundler.Build(Array.Empty<LogEntry>(), "1.6.0", "v1.0.0-beta", new List<BundlePayload.ModInfo>());
        Assert.Equal("1.6.0", p.RimWorldVersion);
        Assert.Equal("v1.0.0-beta", p.FrameworkVersion);
        Assert.Empty(p.Entries);
    }

    [Fact]
    public void Build_MapsLogEntryToDto()
    {
        IReadOnlyDictionary<string, object?> ctx = new Dictionary<string, object?> { ["Name"] = "alice" };
        LogEntry e = MakeEntry(renderedMessage: "hi alice", channel: "test", level: LogLevel.Info, ctx: ctx);
        BundlePayload p = Bundler.Build(new[] { e }, "1.6", "v", new List<BundlePayload.ModInfo>());

        Assert.Single(p.Entries);
        BundlePayload.EntryDto dto = p.Entries[0];
        Assert.Equal("test", dto.Channel);
        Assert.Equal("Info", dto.Level);
        Assert.Equal("hi alice", dto.Msg);
        Assert.NotNull(dto.Ctx);
        Assert.Equal("alice", dto.Ctx!["Name"]);
    }

    [Fact]
    public void Build_NullContext_ProducesNullCtx()
    {
        LogEntry e = MakeEntry(ctx: null);
        BundlePayload p = Bundler.Build(new[] { e }, "x", "y", new List<BundlePayload.ModInfo>());

        Assert.Null(p.Entries[0].Ctx);
    }

    [Fact]
    public void Build_RichTextStrippedFromMessage()
    {
        LogEntry e = MakeEntry(renderedMessage: "<color=red>danger</color> text");
        BundlePayload p = Bundler.Build(new[] { e }, "x", "y", new List<BundlePayload.ModInfo>());

        Assert.Equal("danger text", p.Entries[0].Msg);
    }

    [Fact]
    public void Build_ExceptionPopulatesStack()
    {
        Exception ex = new InvalidOperationException("kaboom");
        LogEntry e = MakeEntry(ex: ex);
        BundlePayload p = Bundler.Build(new[] { e }, "x", "y", new List<BundlePayload.ModInfo>());

        Assert.NotNull(p.Entries[0].Stack);
        Assert.Contains("kaboom", p.Entries[0].Stack!);
    }

    [Fact]
    public void Build_StackTraceFieldTakesPrecedenceOverException()
    {
        Exception ex = new InvalidOperationException("ex message");
        LogEntry e = MakeEntry(ex: ex, stackTrace: "manual stack trace line");
        BundlePayload p = Bundler.Build(new[] { e }, "x", "y", new List<BundlePayload.ModInfo>());

        Assert.Equal("manual stack trace line", p.Entries[0].Stack);
    }

    [Fact]
    public void Build_CallerProvidedSource_FormatsFileAndLine()
    {
        SourceLocation loc = new SourceLocation("Foo.cs", 42, "Bar");
        LogEntry e = MakeEntry(source: loc);
        BundlePayload p = Bundler.Build(new[] { e }, "x", "y", new List<BundlePayload.ModInfo>());

        Assert.Equal("Foo.cs:42", p.Entries[0].Source);
    }

    [Fact]
    public void Build_UnknownSource_ProducesEmptyString()
    {
        LogEntry e = MakeEntry(source: SourceLocation.Empty);
        BundlePayload p = Bundler.Build(new[] { e }, "x", "y", new List<BundlePayload.ModInfo>());

        Assert.Equal("", p.Entries[0].Source);
    }
}
