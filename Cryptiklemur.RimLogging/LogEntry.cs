using System;
using System.Collections.Generic;
using Cryptiklemur.RimLogging.Capture;

namespace Cryptiklemur.RimLogging;

/// <summary>
/// Immutable payload delivered to every sink. Carries both the unrendered message
/// template and the fully-rendered string so structured sinks can extract typed
/// fields from <see cref="Context"/> while text sinks can write
/// <see cref="RenderedMessage"/> directly.
/// </summary>
public sealed class LogEntry
{
    /// <summary>Gets the UTC timestamp at which the log call was made.</summary>
    public DateTime Timestamp { get; }

    /// <summary>Gets the severity level of this entry.</summary>
    public LogLevel Level { get; }

    /// <summary>Gets the channel (dot-separated category) this entry was emitted on.</summary>
    public string Channel { get; }

    /// <summary>
    /// Gets the unrendered message template, e.g. <c>"died at {Hp}hp"</c>.
    /// Structured sinks use this together with <see cref="Context"/> to extract
    /// typed fields.
    /// </summary>
    public string MessageTemplate { get; }

    /// <summary>
    /// Gets the fully-rendered message string, e.g. <c>"died at 5hp"</c>.
    /// Text sinks write this value directly.
    /// </summary>
    public string RenderedMessage { get; }

    /// <summary>
    /// Gets the structured context dictionary, or <c>null</c> when the call site
    /// did not supply an anonymous-object context.
    /// </summary>
    public IReadOnlyDictionary<string, object?>? Context { get; }

    /// <summary>Gets the source location captured at the call site.</summary>
    public SourceLocation Source { get; }

    /// <summary>
    /// Gets the stack trace string, or <c>null</c>. Populated eagerly on the
    /// sync-bypass path at <see cref="LogLevel.Error"/> and
    /// <see cref="LogLevel.Fatal"/> so the stack is captured before the call
    /// stack unwinds.
    /// </summary>
    public string? StackTrace { get; }

    /// <summary>
    /// Gets the exception associated with this entry, or <c>null</c> when the
    /// call site used a non-exception overload.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Initializes a new <see cref="LogEntry"/> with all fields.
    /// </summary>
    /// <param name="timestamp">UTC timestamp of the log call.</param>
    /// <param name="level">Severity level.</param>
    /// <param name="channel">Dot-separated channel name. Must not be <c>null</c>.</param>
    /// <param name="messageTemplate">Unrendered message template; <c>null</c> is treated as <see cref="string.Empty"/>.</param>
    /// <param name="renderedMessage">Fully-rendered message string; <c>null</c> is treated as <see cref="string.Empty"/>.</param>
    /// <param name="context">Structured context dictionary, or <c>null</c>.</param>
    /// <param name="source">Source location captured at the call site.</param>
    /// <param name="stackTrace">Eagerly-captured stack trace string, or <c>null</c>.</param>
    /// <param name="exception">Associated exception, or <c>null</c>.</param>
    public LogEntry(
        DateTime timestamp,
        LogLevel level,
        string channel,
        string messageTemplate,
        string renderedMessage,
        IReadOnlyDictionary<string, object?>? context,
        SourceLocation source,
        string? stackTrace,
        Exception? exception)
    {
        Timestamp = timestamp;
        Level = level;
        Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        MessageTemplate = messageTemplate ?? string.Empty;
        RenderedMessage = renderedMessage ?? string.Empty;
        Context = context;
        Source = source;
        StackTrace = stackTrace;
        Exception = exception;
    }
}
