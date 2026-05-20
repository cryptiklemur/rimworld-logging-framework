using System;
using Cryptiklemur.RimLogging.Format;

namespace Cryptiklemur.RimLogging.Sinks;

/// <summary>
/// Writes log entries to a per-session plaintext file, stripping rich text tags,
/// and deletes the oldest files beyond the configured retention count.
/// </summary>
public sealed class RollingTextFileSink : ILogSink
{
    private readonly System.IO.StreamWriter _writer;
    private readonly object _lock = new object();
    private bool _disposed;

    /// <inheritdoc/>
    public string Name => "RollingText";

    /// <summary>Gets or sets the minimum level; entries below this level are dropped.</summary>
    public LogLevel MinLevel { get; set; }

    /// <summary>Gets or sets the format template used to render each log line.</summary>
    public string FormatTemplate { get; set; } = DefaultFormat.Default;

    /// <summary>Gets the full path of the log file created for this session.</summary>
    public string FilePath { get; }

    /// <summary>
    /// Initializes a new <see cref="RollingTextFileSink"/>.
    /// </summary>
    /// <param name="logDirectory">Directory in which log files are created; created if absent.</param>
    /// <param name="retainCount">Maximum number of log files to keep; oldest beyond this count are deleted.</param>
    /// <param name="minLevel">Entries below this level are silently dropped.</param>
    public RollingTextFileSink(string logDirectory, int retainCount = 5, LogLevel minLevel = LogLevel.Trace)
    {
        if (!System.IO.Directory.Exists(logDirectory))
            System.IO.Directory.CreateDirectory(logDirectory);

        ApplyRetention(logDirectory, retainCount);

        string stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture);
        FilePath = System.IO.Path.Combine(logDirectory,
            $"RimLogging-{stamp}-{System.Diagnostics.Process.GetCurrentProcess().Id}.log");
        System.IO.FileStream fs = new System.IO.FileStream(FilePath, System.IO.FileMode.Append,
            System.IO.FileAccess.Write, System.IO.FileShare.ReadWrite);
        _writer = new System.IO.StreamWriter(fs) { AutoFlush = false };
        MinLevel = minLevel;
    }

    /// <inheritdoc/>
    public void Write(LogEntry entry)
    {
        if (entry.Level < MinLevel) return;
        if (_disposed) return;
        string line = DefaultFormat.Render(FormatTemplate, entry, stripRichText: true);
        lock (_lock)
        {
            if (_disposed) return;
            _writer.WriteLine(line);
            if (entry.Level >= LogLevel.Error) _writer.Flush();
        }
    }

    /// <inheritdoc/>
    public void Flush()
    {
        lock (_lock) { if (!_disposed) _writer.Flush(); }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed) return;
            _disposed = true;
            try { _writer.Flush(); _writer.Dispose(); } catch { }
        }
    }

    private static void ApplyRetention(string dir, int retain)
    {
        string[] files = System.IO.Directory.GetFiles(dir, "RimLogging-*.log");
        if (files.Length <= retain) return;
        Array.Sort(files, (string a, string b) => string.Compare(a, b, StringComparison.Ordinal));
        for (int i = 0; i < files.Length - retain; i++)
        {
            try { System.IO.File.Delete(files[i]); } catch { }
        }
    }
}
