using System.Collections.Generic;
using Cryptiklemur.RimLogging;
using Verse;

namespace Cryptiklemur.RimLogging.Settings;

public sealed class LoggingSettings : ModSettings
{
    public LogLevel globalMinLevel = LogLevel.Info;
    public string logDirectory = "";
    public int retentionCount = 5;
    public string proxyUrl = "https://rimlogging-bundle.cryptiklemur.workers.dev/v1/bundle";
    public List<string> filterPresetNames = new();
    public List<string> filterPresetExpressions = new();

    public override void ExposeData()
    {
        Scribe_Values.Look(ref globalMinLevel, "globalMinLevel", LogLevel.Info);
        Scribe_Values.Look(ref logDirectory, "logDirectory", "");
        Scribe_Values.Look(ref retentionCount, "retentionCount", 5);
        Scribe_Values.Look(ref proxyUrl, "proxyUrl", "https://rimlogging-bundle.cryptiklemur.workers.dev/v1/bundle");
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
