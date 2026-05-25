using HarmonyLib;
using CryptikLemur.RimLogging.Capture;
using CryptikLemur.RimLogging.Pipeline;

namespace CryptikLemur.RimLogging.Hijack;

[HarmonyPatch(typeof(Verse.Log), nameof(Verse.Log.Warning), new[] { typeof(string) })]
internal static class VerseLog_Warning_Patch
{
    static bool Prefix(string text)
    {
        if (ReentryGuard.IsInsideSink) return true;
        (string channel, string? mod) = VerseLogPatchHelpers.ResolveCaller();
        Log.EmitCaptured(LogLevel.Warn, channel, text, mod: mod);
        return false;
    }
}
