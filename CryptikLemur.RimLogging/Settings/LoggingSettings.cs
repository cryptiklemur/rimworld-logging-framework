using System.Collections.Generic;
using CryptikLemur.RimLogging;
using Verse;

namespace CryptikLemur.RimLogging.Settings;

/// <summary>Persisted RimWorld <see cref="ModSettings"/> for the logging framework (min level, output directory, retention, bundle proxy, filter presets).</summary>
public sealed class LoggingSettings : ModSettings
{
    /// <summary>Global minimum log level; entries below this are dropped.</summary>
    public LogLevel globalMinLevel = LogLevel.Info;

    /// <summary>Directory where log files are written; empty until normalized to the default.</summary>
    public string logDirectory = "";

    /// <summary>Number of rotated log files to retain.</summary>
    public int retentionCount = 5;

    /// <summary>Endpoint URL used when uploading bug-report bundles.</summary>
    public string proxyUrl = "https://rimlogging-bundle.cryptiklemur.workers.dev/v1/bundle";

    /// <summary>Display names of saved filter presets, parallel to <see cref="filterPresetExpressions"/>.</summary>
    public List<string> filterPresetNames = new();

    /// <summary>Filter DSL expressions for saved presets, parallel to <see cref="filterPresetNames"/>.</summary>
    public List<string> filterPresetExpressions = new();

    /// <summary>When <c>true</c>, every emitted entry captures and stores a formatted stack trace. Defaults to <c>true</c>.</summary>
    public bool captureStackTraces = true;

    /// <summary>Serializes and deserializes the settings via RimWorld's Scribe system, applying defaults on post-load.</summary>
    public override void ExposeData()
    {
        Scribe_Values.Look(ref globalMinLevel, "globalMinLevel", LogLevel.Info);
        Scribe_Values.Look(ref logDirectory, "logDirectory", "");
        Scribe_Values.Look(ref retentionCount, "retentionCount", 5);
        Scribe_Values.Look(ref proxyUrl, "proxyUrl", "https://rimlogging-bundle.cryptiklemur.workers.dev/v1/bundle");
        Scribe_Values.Look(ref captureStackTraces, "captureStackTraces", true);
        Scribe_Collections.Look(ref filterPresetNames, "filterPresetNames", LookMode.Value);
        Scribe_Collections.Look(ref filterPresetExpressions, "filterPresetExpressions", LookMode.Value);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            filterPresetNames ??= new List<string>();
            filterPresetExpressions ??= new List<string>();
            if (string.IsNullOrEmpty(logDirectory)) logDirectory = LogDirectory.Default;
        }
    }
}
