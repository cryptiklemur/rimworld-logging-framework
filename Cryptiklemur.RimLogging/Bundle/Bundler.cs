using System;
using System.Collections.Generic;
using Cryptiklemur.RimLogging.Format;

namespace Cryptiklemur.RimLogging.Bundle;

public static class Bundler
{
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
                Ts = e.Timestamp.ToString("o"),
                Level = e.Level.ToString(),
                Channel = e.Channel,
                Source = e.Source.IsCallerProvided ? $"{e.Source.File}:{e.Source.Line}" : "",
                Msg = RichText.Strip(e.RenderedMessage),
                Ctx = CopyContext(e.Context),
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
