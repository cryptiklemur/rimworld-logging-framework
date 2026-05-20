using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cryptiklemur.RimLogging.Capture;
using Cryptiklemur.RimLogging.Format;

namespace Cryptiklemur.RimLogging;

/// <summary>Entry point for emitting log messages through the RimLogging framework.</summary>
public static class Log
{
    /// <summary>Name of the default log channel used when no channel is specified.</summary>
    public const string DefaultChannel = "default";

    /// <summary>Log at Info using a templated message and positional args (default channel).</summary>
    public static void Info(
        string template,
        params object?[] args)
    {
        InfoImpl(DefaultChannel, template, args, structuredContext: null, exception: null,
                 SourceLocation.Empty, line: 0, file: string.Empty);
    }

    /// <summary>Log at Info using a templated message, positional args, and compiler-supplied caller info (default channel).</summary>
    public static void Info(
        string template,
        object?[] args,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
        => InfoImpl(DefaultChannel, template, args, null, null, SourceLocation.Empty, line, file);

    /// <summary>Log at Info using a plain message and an anonymous-object context (default channel).</summary>
    public static void Info(
        string message,
        object context,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
        => InfoImpl(DefaultChannel, message, args: null, structuredContext: context, exception: null,
                    SourceLocation.Empty, line, file);

    /// <summary>Log at Info using an explicit channel, a templated message, and optional positional args.</summary>
    public static void Info(
        string channel,
        string template,
        object?[]? args = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
        => InfoImpl(channel, template, args, null, null, SourceLocation.Empty, line, file);

    /// <summary>Log at Info using an explicit channel, a plain message, and an anonymous-object context.</summary>
    public static void Info(
        string channel,
        string message,
        object context,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
        => InfoImpl(channel, message, args: null, structuredContext: context, exception: null,
                    SourceLocation.Empty, line, file);

    private static void InfoImpl(
        string channel,
        string template,
        object?[]? args,
        object? structuredContext,
        Exception? exception,
        SourceLocation explicitSource,
        int line,
        string file)
    {
        EmitInternal(LogLevel.Info, channel, template, args, structuredContext, exception,
                     explicitSource, line, file);
    }

    private static void EmitInternal(
        LogLevel level,
        string channel,
        string template,
        object?[]? args,
        object? structuredContext,
        Exception? exception,
        SourceLocation explicitSource,
        int line,
        string file)
    {
        // 1. Global gate — cheapest possible short-circuit. NO formatting, NO reflection.
        if (level < Logging.GlobalMinLevel) return;

        // 2. Source location — caller-info first, stack walk fallback.
        SourceLocation src;
        if (line > 0 && !string.IsNullOrEmpty(file))
            src = new SourceLocation(NormalisePath(file), line, null);
        else if (explicitSource.IsCallerProvided)
            src = explicitSource;
        else
            src = StackWalker.WalkOnce();

        // 3. Render template + capture context.
        string rendered;
        IReadOnlyDictionary<string, object?>? ctx = null;
        if (args != null && args.Length > 0)
        {
            Format.MessageTemplate t = TemplateCache.Get(template);
            (rendered, ctx) = t.Render(args);
        }
        else
        {
            rendered = template ?? string.Empty;
        }
        if (structuredContext != null)
        {
            IReadOnlyDictionary<string, object?>? captured = StructuredContext.Capture(structuredContext);
            if (captured != null)
            {
                if (ctx == null) ctx = captured;
                else
                {
                    Dictionary<string, object?> merged = new Dictionary<string, object?>(ctx.Count + captured.Count);
                    foreach (KeyValuePair<string, object?> kv in ctx) merged[kv.Key] = kv.Value;
                    foreach (KeyValuePair<string, object?> kv in captured) merged[kv.Key] = kv.Value;
                    ctx = merged;
                }
            }
        }

        LogEntry entry = new LogEntry(
            timestamp: DateTime.UtcNow,
            level: level,
            channel: channel ?? DefaultChannel,
            messageTemplate: template ?? string.Empty,
            renderedMessage: rendered,
            context: ctx,
            source: src,
            stackTrace: null,
            exception: exception);

        Logging.Emit(entry);
    }

    private static string NormalisePath(string file)
    {
        // Same cleanup as StackWalker, but for the cooperative caller-info path.
        string clean = file.Replace('\\', '/');
        int idx = clean.LastIndexOf('/');
        if (idx >= 0) clean = clean.Substring(idx + 1);
        return clean.Replace(".cs", string.Empty);
    }
}
