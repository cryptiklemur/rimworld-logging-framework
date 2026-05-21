using System;
using System.Collections.Generic;
using Cryptiklemur.RimLogging;
using Cryptiklemur.RimLogging.Sinks;

namespace Cryptiklemur.RimLogging.UI;

internal sealed class UISink : ILogSink
{
    private readonly object _lock = new();
    private readonly LogEntry[] _ring = new LogEntry[4096];
    private int _writeIndex;
    private int _count;

    public event Action<LogEntry>? EntryAdded;

    public string Name => "UISink";
    public LogLevel MinLevel { get; set; } = LogLevel.Trace;

    public IReadOnlyList<LogEntry> Snapshot()
    {
        lock (_lock)
        {
            LogEntry[] snap = new LogEntry[_count];
            for (int i = 0; i < _count; i++)
            {
                int idx = (_writeIndex - _count + i + _ring.Length) % _ring.Length;
                snap[i] = _ring[idx];
            }
            return snap;
        }
    }

    public void Write(LogEntry entry)
    {
        if (entry.Level < MinLevel) return;
        lock (_lock)
        {
            _ring[_writeIndex] = entry;
            _writeIndex = (_writeIndex + 1) % _ring.Length;
            if (_count < _ring.Length) _count++;
        }
        EntryAdded?.Invoke(entry);
    }

    public void Flush() { }
    public void Dispose() { }
}
