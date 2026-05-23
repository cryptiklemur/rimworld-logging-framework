using CryptikLemur.RimLogging.Bootstrap;
using CryptikLemur.RimLogging.Capture;

namespace CryptikLemur.RimLogging.Hijack;

internal static class HijackBootstrap
{
    private static volatile bool _installed;
    private static HarmonyLib.Harmony? _harmony;

    internal static bool Install()
    {
        if (_installed) return true;
        if (DegradedMode.AnotherCopyPresent()) return false;

        AssemblyChannelCache.ResolverHook = AssemblyChannelResolver.Resolve;
        AssemblyChannelCache.OnResolverError = (asm, ex) =>
            Verse.Log.Warning($"[RimLogging] channel resolver failed for '{asm.GetName().Name}': {ex.GetType().Name}: {ex.Message}");
        ModNameCache.Provider = ModNameMapProvider.Build;
        Sinks.VerseLogSink.VanillaWriter = VanillaBufferWriteback.Write;
        VerseLogBackfill.Drain();
        _harmony = new HarmonyLib.Harmony("CryptikLemur.RimLogging");
        _harmony.PatchAll(typeof(HijackBootstrap).Assembly);
        UnityLogBridge.Install();
        _installed = true;
        return true;
    }

    internal static void UninstallForTests()
    {
        UnityLogBridge.Uninstall();
        _harmony?.UnpatchAll("CryptikLemur.RimLogging");
        Sinks.VerseLogSink.VanillaWriter = null;
        _installed = false;
    }
}
