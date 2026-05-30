using System.Linq;

namespace CryptikLemur.RimLogging.Bootstrap;

internal static class DegradedMode
{
    private static volatile bool _detected;

    internal static bool IsPresent => _detected;

    internal static bool AnotherCopyPresent()
    {
        try
        {
            System.Reflection.MethodInfo? target = typeof(Verse.Log)
                .GetMethod(nameof(Verse.Log.Message), new[] { typeof(string) });
            if (target == null) return false;

            HarmonyLib.Patches? patches = HarmonyLib.Harmony.GetPatchInfo(target);
            if (patches == null) return false;

            if (patches.Prefixes.Any(p => p.owner.StartsWith("CryptikLemur.RimLogging", System.StringComparison.OrdinalIgnoreCase)))
            {
                _detected = true;
                return true;
            }
        }
        catch
        {
            // Harmony introspection failed; assume no conflicting copy and run normally.
        }
        return false;
    }
}
