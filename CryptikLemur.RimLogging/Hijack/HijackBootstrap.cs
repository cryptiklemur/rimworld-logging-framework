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
        ModNameCache.FolderProvider = ModNameMapProvider.BuildFolders;
        ModNameCache.OnProviderError = ex =>
            Verse.Log.Warning($"[RimLogging] mod-name provider failed: {ex.GetType().Name}: {ex.Message}");
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
        AssemblyChannelCache.OnResolverError = null;
        ModNameCache.OnProviderError = null;
        _installed = false;
    }
}
