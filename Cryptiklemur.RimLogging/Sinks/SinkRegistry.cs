using System.Collections.Generic;
using Cryptiklemur.RimLogging.Pipeline;

namespace Cryptiklemur.RimLogging.Sinks;

/// <summary>Central registry that maintains the active sink list and fans out log entries to all registered sinks.</summary>
internal static class SinkRegistry
{
    private static readonly List<ILogSink> _sinks = new List<ILogSink>();
    private static readonly System.Threading.ReaderWriterLockSlim _lock = new System.Threading.ReaderWriterLockSlim();

    public static void Register(ILogSink sink)
    {
        _lock.EnterWriteLock();
        try { _sinks.Add(sink); }
        finally { _lock.ExitWriteLock(); }
    }

    public static bool Remove(ILogSink sink)
    {
        _lock.EnterWriteLock();
        try { return _sinks.Remove(sink); }
        finally { _lock.ExitWriteLock(); }
    }

    public static IReadOnlyList<ILogSink> Snapshot()
    {
        _lock.EnterReadLock();
        try { return _sinks.ToArray(); }
        finally { _lock.ExitReadLock(); }
    }

    public static void DispatchSynchronously(LogEntry entry)
    {
        if (ReentryGuard.IsInsideSink) return;
        IReadOnlyList<ILogSink> snap = Snapshot();
        using (ReentryGuard.Enter())
        {
            for (int i = 0; i < snap.Count; i++)
            {
                try { snap[i].Write(entry); }
                catch { }
            }
        }
    }

    public static void FlushAll()
    {
        IReadOnlyList<ILogSink> snap = Snapshot();
        for (int i = 0; i < snap.Count; i++)
        {
            try { snap[i].Flush(); } catch { }
        }
    }

    public static void DisposeAll()
    {
        IReadOnlyList<ILogSink> snap = Snapshot();
        _lock.EnterWriteLock();
        try { _sinks.Clear(); }
        finally { _lock.ExitWriteLock(); }
        for (int i = 0; i < snap.Count; i++)
        {
            try { snap[i].Dispose(); } catch { }
        }
    }
}
