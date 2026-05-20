using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using Cryptiklemur.RimLogging.Format;

namespace Cryptiklemur.RimLogging.Sinks;

/// <summary>
/// Writes log entries as NDJSON (one JSON object per line) to a per-session file,
/// and deletes the oldest files beyond the configured retention count.
/// </summary>
public sealed class RollingJsonFileSink : ILogSink
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly System.IO.StreamWriter _writer;
    private readonly object _lock = new object();
    private bool _disposed;

    /// <inheritdoc/>
    public string Name => "RollingJson";

    /// <summary>Gets or sets the minimum level; entries below this level are dropped.</summary>
    public LogLevel MinLevel { get; set; }

    /// <summary>Gets the full path of the NDJSON file created for this session.</summary>
    public string FilePath { get; }

    /// <summary>
    /// Initializes a new <see cref="RollingJsonFileSink"/>.
    /// </summary>
    /// <param name="logDirectory">Directory in which log files are created; created if absent.</param>
    /// <param name="retainCount">Maximum number of log files to keep; oldest beyond this count are deleted.</param>
    /// <param name="minLevel">Entries below this level are silently dropped.</param>
    public RollingJsonFileSink(string logDirectory, int retainCount = 5, LogLevel minLevel = LogLevel.Trace)
    {
        if (!System.IO.Directory.Exists(logDirectory))
            System.IO.Directory.CreateDirectory(logDirectory);

        ApplyRetention(logDirectory, retainCount);

        string stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        FilePath = System.IO.Path.Combine(logDirectory,
            $"RimLogging-{stamp}-{System.Diagnostics.Process.GetCurrentProcess().Id}.ndjson");
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

        string line = BuildJson(entry);
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

    private static string BuildJson(LogEntry entry)
    {
        string? src = entry.Source.IsCallerProvided
            ? $"{entry.Source.File}:{entry.Source.Line}"
            : null;

        Dictionary<string, object?> row = new Dictionary<string, object?>
        {
            ["ts"] = entry.Timestamp.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture),
            ["level"] = entry.Level.ToString().ToUpperInvariant(),
            ["channel"] = entry.Channel,
            ["src"] = src,
            ["msg"] = RichText.Strip(entry.RenderedMessage),
            ["tmpl"] = entry.MessageTemplate,
            ["ctx"] = BuildCtx(entry.Context),
            ["stack"] = entry.StackTrace,
            ["exc"] = BuildExc(entry.Exception),
        };

        return JsonSerializer.Serialize(row, _jsonOptions);
    }

    private static Dictionary<string, object?>? BuildCtx(System.Collections.Generic.IReadOnlyDictionary<string, object?>? context)
    {
        if (context == null || context.Count == 0) return null;
        Dictionary<string, object?> result = new Dictionary<string, object?>();
        foreach (System.Collections.Generic.KeyValuePair<string, object?> pair in context)
            result[pair.Key] = pair.Value;
        return result;
    }

    private static Dictionary<string, object?>? BuildExc(Exception? ex)
    {
        if (ex == null) return null;

        return new Dictionary<string, object?>
        {
            ["type"] = ex.GetType().FullName,
            ["message"] = ex.Message,
            ["stack"] = ex.StackTrace,
        };
    }

    private static void ApplyRetention(string dir, int retain)
    {
        string[] files = System.IO.Directory.GetFiles(dir, "RimLogging-*.ndjson");
        if (files.Length <= retain) return;
        Array.Sort(files, (string a, string b) => string.Compare(a, b, StringComparison.Ordinal));
        for (int i = 0; i < files.Length - retain; i++)
        {
            try { System.IO.File.Delete(files[i]); } catch { }
        }
    }
}
