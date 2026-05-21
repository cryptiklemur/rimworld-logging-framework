using HarmonyLib;
using Cryptiklemur.RimLogging;
using Cryptiklemur.RimLogging.Sinks;
using Cryptiklemur.RimLogging.UI.Window;

namespace Cryptiklemur.RimLogging.UI.ReplaceVanillaLog;

// Harmony patch: intercept Verse.Log.TryOpenLogWindow() and show our LogViewerWindow instead.
// We iterate Logging.RegisteredSinks (not Logging.Sinks, which would shadow the Sinks namespace).
[HarmonyPatch(typeof(Verse.Log), nameof(Verse.Log.TryOpenLogWindow))]
internal static class Log_TryOpenLogWindow_Patch
{
    static bool Prefix()
    {
        if (VanillaBridge.ForceVanilla) return true;
        UISink? sink = FindUISink();
        if (sink == null) return true;
        Verse.Find.WindowStack.Add(new LogViewerWindow(sink));
        return false;
    }

    private static UISink? FindUISink()
    {
        foreach (ILogSink s in Logging.RegisteredSinks)
        {
            if (s is UISink u) return u;
        }
        return null;
    }
}
