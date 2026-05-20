using System;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using Cryptiklemur.RimLogging.Capture;
using Cryptiklemur.RimLogging.Pipeline;

namespace Cryptiklemur.RimLogging.Hijack;

/// <summary>Harmony prefix on <c>Verse.Log.Message(string)</c>; routes through framework when not in re-entry.</summary>
[HarmonyPatch(typeof(Verse.Log), nameof(Verse.Log.Message), new[] { typeof(string) })]
internal static class VerseLog_Message_Patch
{
    static bool Prefix(string text)
    {
        if (ReentryGuard.IsInsideSink) return true;
        Log.Info(VerseLogPatchHelpers.ResolveCallerChannel(), text);
        return false;
    }
}

/// <summary>Harmony prefix on <c>Verse.Log.Warning(string)</c>; routes through framework when not in re-entry.</summary>
[HarmonyPatch(typeof(Verse.Log), nameof(Verse.Log.Warning), new[] { typeof(string) })]
internal static class VerseLog_Warning_Patch
{
    static bool Prefix(string text)
    {
        if (ReentryGuard.IsInsideSink) return true;
        Log.Warn(VerseLogPatchHelpers.ResolveCallerChannel(), text);
        return false;
    }
}

/// <summary>Harmony prefix on <c>Verse.Log.Error(string)</c>; routes through framework when not in re-entry.</summary>
[HarmonyPatch(typeof(Verse.Log), nameof(Verse.Log.Error), new[] { typeof(string) })]
internal static class VerseLog_Error_Patch
{
    static bool Prefix(string text)
    {
        if (ReentryGuard.IsInsideSink) return true;
        Log.Error(VerseLogPatchHelpers.ResolveCallerChannel(), text);
        return false;
    }
}

/// <summary>Shared helpers for the <c>VerseLog_*_Patch</c> classes.</summary>
internal static class VerseLogPatchHelpers
{
    /// <summary>
    /// Walks the call stack (skipping Harmony invoker frames and our own pipeline frames) to find
    /// the first external assembly, then returns its channel name via <see cref="AssemblyChannelCache.Resolve"/>.
    /// Falls back to <c>"Vanilla"</c> when no external frame is found.
    /// </summary>
    public static string ResolveCallerChannel()
    {
        StackTrace st = new StackTrace(2, false);
        for (int i = 0; i < st.FrameCount; i++)
        {
            MethodBase? m = st.GetFrame(i)?.GetMethod();
            string? ns = m?.DeclaringType?.FullName;
            if (ns == null) continue;
            if (ns.StartsWith("HarmonyLib.", StringComparison.Ordinal)) continue;
            if (ns.StartsWith("Cryptiklemur.RimLogging.", StringComparison.Ordinal)) continue;
            return AssemblyChannelCache.Resolve(m!.DeclaringType!.Assembly);
        }
        return "Vanilla";
    }
}
