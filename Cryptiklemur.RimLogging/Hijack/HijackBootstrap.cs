using Cryptiklemur.RimLogging.Bootstrap;

namespace Cryptiklemur.RimLogging.Hijack;

internal static class HijackBootstrap
{
    private static volatile bool _installed;
    private static HarmonyLib.Harmony? _harmony;

    public static bool Install()
    {
        if (_installed) return true;
        if (DegradedMode.AnotherCopyPresent()) return false;

        _harmony = new HarmonyLib.Harmony("cryptiklemur.rimlogging");
        _harmony.PatchAll(typeof(HijackBootstrap).Assembly);
        UnityLogBridge.Install();
        _installed = true;
        return true;
    }

    internal static void UninstallForTests()
    {
        UnityLogBridge.Uninstall();
        _harmony?.UnpatchAll("cryptiklemur.rimlogging");
        _installed = false;
    }
}
