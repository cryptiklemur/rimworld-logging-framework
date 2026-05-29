using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using CryptikLemur.RimLogging.Capture;
using CryptikLemur.RimLogging.Pipeline;

namespace CryptikLemur.RimLogging.Hijack;

[HarmonyPatch(typeof(Verse.Log), nameof(Verse.Log.Error), typeof(string))]
internal static class VerseLog_Error_Patch
{
    [SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed",
        Justification = "Invoked by Harmony via reflection")]
    static bool Prefix(string text)
    {
        if (ReentryGuard.IsInsideSink) return true;
        (string channel, string? mod) = VerseLogPatchHelpers.ResolveCaller();
        Log.EmitCaptured(LogLevel.Error, channel, text, mod: mod);
        return false;
    }
}
