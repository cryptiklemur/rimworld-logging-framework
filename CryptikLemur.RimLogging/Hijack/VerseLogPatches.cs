using System;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using CryptikLemur.RimLogging.Capture;
using CryptikLemur.RimLogging.Pipeline;

namespace CryptikLemur.RimLogging.Hijack;

/// <summary>Harmony prefix on <c>Verse.Log.Message(string)</c>; routes through framework when not in re-entry.</summary>
[HarmonyPatch(typeof(Verse.Log), nameof(Verse.Log.Message), new[] { typeof(string) })]
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

/// <summary>Harmony prefix on <c>Verse.Log.Warning(string)</c>; routes through framework when not in re-entry.</summary>
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

/// <summary>Harmony prefix on <c>Verse.Log.Error(string)</c>; routes through framework when not in re-entry.</summary>
[HarmonyPatch(typeof(Verse.Log), nameof(Verse.Log.Error), new[] { typeof(string) })]
internal static class VerseLog_Error_Patch
{
    static bool Prefix(string text)
    {
        if (ReentryGuard.IsInsideSink) return true;
        (string channel, string? mod) = VerseLogPatchHelpers.ResolveCaller();
        Log.EmitCaptured(LogLevel.Error, channel, text, mod: mod);
        return false;
    }
}

/// <summary>Shared helpers for the <c>VerseLog_*_Patch</c> classes.</summary>
internal static class VerseLogPatchHelpers
{
    /// <summary>
    /// Walks the call stack (skipping Harmony invoker frames and our own pipeline frames) to find
    /// the first external assembly, then returns its channel name via <see cref="AssemblyChannelCache.Resolve"/>
    /// together with its mod name via <see cref="ModNameCache.ForAssembly"/>. Falls back to
    /// <c>("Vanilla", null)</c> when no external frame is found.
    /// </summary>
    internal static (string Channel, string? Mod) ResolveCaller()
    {
        StackTrace st = new StackTrace(2, false);
        for (int i = 0; i < st.FrameCount; i++)
        {
            MethodBase? m = st.GetFrame(i)?.GetMethod();
            Type? dt = m?.DeclaringType;
            string? ns = dt?.FullName;
            string? asm = dt?.Assembly.GetName().Name;
            if (CallerFrameClassifier.IsInternalFrame(ns, asm)) continue;
            return (AssemblyChannelCache.Resolve(dt!.Assembly), ModNameCache.ForAssembly(dt.Assembly));
        }
        return ("Vanilla", null);
    }
}
