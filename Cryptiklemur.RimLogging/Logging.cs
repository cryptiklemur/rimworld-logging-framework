using System;
using Cryptiklemur.RimLogging.Pipeline;

namespace Cryptiklemur.RimLogging;

/// <summary>Entry point for emitting log entries into the framework.</summary>
public static class Logging
{
    private static MpscQueue<LogEntry>? _queue;
    private static BackgroundDrain? _drain;
#pragma warning disable CS0649
    internal static Action<LogEntry>? _dispatchSyncOverride; // tests only
    internal static Action<BackgroundDrain>? _installShutdownHook;
#pragma warning restore CS0649

    public static LogLevel GlobalMinLevel { get; set; } = LogLevel.Trace;

    /// <summary>Emit a log entry, applying the global minimum-level filter.</summary>
    internal static void Emit(LogEntry entry)
    {
        if (entry.Level < GlobalMinLevel) return;

        if (SynchronousBypass.ShouldBypass(entry.Level) || _drain == null)
        {
            DispatchSync(entry);
            return;
        }
        _drain.Enqueue(entry);
    }

    internal static void DispatchSync(LogEntry entry)
    {
        if (_dispatchSyncOverride != null) { _dispatchSyncOverride(entry); return; }
        // TODO Phase 4: wire SinkRegistry.DispatchSynchronously(entry);
    }

    /// <summary>Lazy initializer used by internal paths.</summary>
    internal static void EnsureStarted()
    {
        if (_queue != null) return;
        _queue = new MpscQueue<LogEntry>(65536);
        _drain = new BackgroundDrain(_queue, DispatchSync);
    }

    /// <summary>
    /// Explicit public lifecycle entry point. Idempotent.
    /// Invokes <see cref="_installShutdownHook"/> if non-null (wired by Bootstrap; null in tests).
    /// </summary>
    public static void Init()
    {
        EnsureStarted();
        if (_drain != null) _installShutdownHook?.Invoke(_drain);
    }

    /// <summary>Tears down the drain and clears queue/drain references. After this, Emit routes synchronously.</summary>
    public static void Shutdown()
    {
        _drain?.Dispose();
        _drain = null;
        _queue = null;
    }

    internal static void StopForTests() => Shutdown();
}
