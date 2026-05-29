using HarmonyLib;
using CryptikLemur.RimLogging.Capture;
using CryptikLemur.RimLogging.Pipeline;

namespace CryptikLemur.RimLogging.Hijack;

[HarmonyPatch(typeof(Verse.Log), nameof(Verse.Log.Message), typeof(string))]
internal static class VerseLog_Message_Patch
{
    static bool Prefix(string text)
    {
        if (ReentryGuard.IsInsideSink) return true;
        (string channel, string? mod) = VerseLogPatchHelpers.ResolveCaller();
        Log.EmitCaptured(LogLevel.Info, channel, text, mod: mod);
        return false;
    }
}
