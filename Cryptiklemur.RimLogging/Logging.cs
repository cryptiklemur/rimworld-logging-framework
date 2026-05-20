namespace Cryptiklemur.RimLogging;

/// <summary>Entry point for emitting log entries into the framework.</summary>
public static class Logging
{
    internal static LogEntry? LastEntry;       // testing hook -- removed in Phase 4
    internal static int EntriesSeen;            // testing hook -- removed in Phase 4

    internal static LogLevel GlobalMinLevel { get; set; } = LogLevel.Trace;

    /// <summary>Emit a log entry, applying the global minimum-level filter.</summary>
    internal static void Emit(LogEntry entry)
    {
        if (entry.Level < GlobalMinLevel) return;
        LastEntry = entry;
        EntriesSeen++;
    }
}
