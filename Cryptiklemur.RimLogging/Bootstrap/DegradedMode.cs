namespace Cryptiklemur.RimLogging.Bootstrap;

// TODO(Phase 10): replace stub with real duplicate-assembly detection (Task 10.4)
internal static class DegradedMode
{
    private static volatile bool _detected;

    public static bool IsPresent => _detected;

    public static bool AnotherCopyPresent()
    {
        try
        {
            System.Reflection.MethodInfo? target = typeof(Verse.Log)
                .GetMethod(nameof(Verse.Log.Message), new[] { typeof(string) });
            if (target == null) return false;

            HarmonyLib.Patches? patches = HarmonyLib.Harmony.GetPatchInfo(target);
            if (patches == null) return false;

            foreach (HarmonyLib.Patch p in patches.Prefixes)
            {
                if (p.owner.StartsWith("cryptiklemur.rimlogging", System.StringComparison.Ordinal))
                {
                    _detected = true;
                    return true;
                }
            }
        }
        catch { }
        return false;
    }
}
