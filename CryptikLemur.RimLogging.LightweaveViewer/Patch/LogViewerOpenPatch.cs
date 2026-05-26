using HarmonyLib;
using Verse;

namespace CryptikLemur.RimLogging.LightweaveViewer;

[HarmonyPatch(typeof(Verse.Log), nameof(Verse.Log.TryOpenLogWindow))]
internal static class LogViewerOpenPatch {
    private static bool Prefix() {
        LightweaveLogSink? sink = LogViewerBoot.Sink;
        WindowStack? windowStack = Find.WindowStack;
        if (sink == null || windowStack == null) {
            return true;
        }
        windowStack.Add(new LogViewerWindow(sink));
        return false;
    }
}
