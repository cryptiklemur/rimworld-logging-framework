using System;
using System.Collections.Generic;
using Cryptiklemur.RimLogging;
using Cryptiklemur.RimLogging.Capture;
using Cryptiklemur.RimLogging.UI;
using Xunit;

namespace Cryptiklemur.RimLogging.UI.Tests;

public sealed class UISinkTests
{
    private static LogEntry MakeEntry(LogLevel level = LogLevel.Info, string msg = "msg")
    {
        return new LogEntry(
            timestamp: DateTime.UtcNow,
            level: level,
            channel: "test",
            messageTemplate: msg,
            renderedMessage: msg,
            context: null,
            source: SourceLocation.Empty,
            stackTrace: null,
            exception: null);
    }

    [Fact]
    public void Write_ThenSnapshot_ReturnsSingleEntry()
    {
        UISink sink = new UISink();
        LogEntry entry = MakeEntry();

        sink.Write(entry);
        IReadOnlyList<LogEntry> snap = sink.Snapshot();

        Assert.Single(snap);
        Assert.Same(entry, snap[0]);
    }

    [Fact]
    public void Write_MultipleEntries_SnapshotInInsertionOrder()
    {
        UISink sink = new UISink();
        LogEntry[] entries = new LogEntry[10];
        for (int i = 0; i < entries.Length; i++)
            entries[i] = MakeEntry(msg: $"msg{i}");

        foreach (LogEntry e in entries)
            sink.Write(e);

        IReadOnlyList<LogEntry> snap = sink.Snapshot();
        Assert.Equal(entries.Length, snap.Count);
        for (int i = 0; i < entries.Length; i++)
            Assert.Same(entries[i], snap[i]);
    }

    [Fact]
    public void Write_OnePastCapacity_DropsOldestEntry()
    {
        UISink sink = new UISink();
        LogEntry first = MakeEntry(msg: "first");
        sink.Write(first);

        for (int i = 1; i < 4096; i++)
            sink.Write(MakeEntry(msg: $"m{i}"));

        LogEntry overflow = MakeEntry(msg: "overflow");
        sink.Write(overflow);

        IReadOnlyList<LogEntry> snap = sink.Snapshot();
        Assert.Equal(4096, snap.Count);
        Assert.DoesNotContain(first, snap);
        Assert.Same(overflow, snap[snap.Count - 1]);
    }

    [Fact]
    public void Write_TwoFullWraps_SnapshotInCorrectOrder()
    {
        UISink sink = new UISink();
        LogEntry[] written = new LogEntry[8200];
        for (int i = 0; i < written.Length; i++)
        {
            written[i] = MakeEntry(msg: $"m{i}");
            sink.Write(written[i]);
        }

        IReadOnlyList<LogEntry> snap = sink.Snapshot();
        Assert.Equal(4096, snap.Count);

        int expectedStart = 8200 - 4096;
        for (int i = 0; i < snap.Count; i++)
            Assert.Same(written[expectedStart + i], snap[i]);
    }

    [Fact]
    public void EntryAdded_FiresOncePerWrite()
    {
        UISink sink = new UISink();
        int count = 0;
        sink.EntryAdded += _ => count++;

        sink.Write(MakeEntry());
        sink.Write(MakeEntry());
        sink.Write(MakeEntry());

        Assert.Equal(3, count);
    }

    [Fact]
    public void EntryAdded_MultipleSubscribers_AllReceiveEntry()
    {
        UISink sink = new UISink();
        List<LogEntry> received1 = new List<LogEntry>();
        List<LogEntry> received2 = new List<LogEntry>();
        sink.EntryAdded += e => received1.Add(e);
        sink.EntryAdded += e => received2.Add(e);

        LogEntry entry = MakeEntry();
        sink.Write(entry);

        Assert.Single(received1);
        Assert.Same(entry, received1[0]);
        Assert.Single(received2);
        Assert.Same(entry, received2[0]);
    }

    [Fact]
    public void Write_BelowMinLevel_DroppedFromRing()
    {
        UISink sink = new UISink();
        sink.MinLevel = LogLevel.Warn;

        sink.Write(MakeEntry(LogLevel.Trace));
        sink.Write(MakeEntry(LogLevel.Debug));
        sink.Write(MakeEntry(LogLevel.Info));

        Assert.Empty(sink.Snapshot());
    }

    [Fact]
    public void Write_BelowMinLevel_DoesNotFireEntryAdded()
    {
        UISink sink = new UISink();
        sink.MinLevel = LogLevel.Warn;
        int count = 0;
        sink.EntryAdded += _ => count++;

        sink.Write(MakeEntry(LogLevel.Trace));
        sink.Write(MakeEntry(LogLevel.Info));

        Assert.Equal(0, count);
    }
}
