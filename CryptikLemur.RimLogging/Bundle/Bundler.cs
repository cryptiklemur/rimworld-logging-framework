using System;
using System.Collections.Generic;
using CryptikLemur.RimLogging.Format;

namespace CryptikLemur.RimLogging.Bundle;

/// <summary>
/// Builds a <see cref="BundlePayload"/> from captured log entries and environment metadata, flattening each
/// <see cref="LogEntry"/> into a serializable <see cref="BundlePayload.EntryDto"/>.
/// </summary>
public static class Bundler
{
    /// <summary>
    /// Constructs a bundle payload from the given log entries and environment metadata. Each entry's timestamp
    /// is formatted as ISO-8601, its message is stripped of rich-text markup, and its source location is
    /// included as <c>file:line</c> only when caller-provided.
    /// </summary>
    /// <param name="entries">The log entries to include.</param>
    /// <param name="rimWorldVersion">The RimWorld game version to record.</param>
    /// <param name="frameworkVersion">The RimLogging framework revision to record.</param>
    /// <param name="mods">The loaded mod list to record.</param>
    /// <returns>A populated <see cref="BundlePayload"/>.</returns>
    public static BundlePayload Build(
        IReadOnlyList<LogEntry> entries,
        string rimWorldVersion,
        string frameworkVersion,
        List<BundlePayload.ModInfo> mods)
    {
        BundlePayload p = new BundlePayload
        {
            RimWorldVersion = rimWorldVersion,
            FrameworkVersion = frameworkVersion,
            Mods = mods,
        };
        for (int i = 0; i < entries.Count; i++)
        {
            LogEntry e = entries[i];
            p.Entries.Add(new BundlePayload.EntryDto
            {
                Timestamp = e.Timestamp.ToString("o"),
                Level = e.Level.ToString(),
                Channel = e.Channel,
                Source = e.Source.IsCallerProvided ? $"{e.Source.File}:{e.Source.Line}" : "",
                Message = RichText.Strip(e.RenderedMessage),
                Context = CopyContext(e.Context),
                Stack = e.StackTrace ?? e.Exception?.ToString(),
            });
        }
        return p;
    }

    private static Dictionary<string, object?>? CopyContext(IReadOnlyDictionary<string, object?>? source)
    {
        if (source == null) return null;
        Dictionary<string, object?> copy = new Dictionary<string, object?>(source.Count);
        foreach (KeyValuePair<string, object?> kv in source)
            copy[kv.Key] = kv.Value;
        return copy;
    }
}
