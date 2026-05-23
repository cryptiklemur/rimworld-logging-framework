using System.Collections.Generic;
using System.Reflection;

namespace CryptikLemur.RimLogging.Hijack;

/// <summary>
/// Verse-aware provider that projects <see cref="Verse.LoadedModManager.RunningMods"/> into an
/// assembly-simple-name to mod-name (About.xml <c>&lt;name&gt;</c>) map. Wired into
/// <see cref="Capture.ModNameCache.Provider"/> by <see cref="HijackBootstrap.Install"/>.
/// </summary>
internal static class ModNameMapProvider
{
    internal static IReadOnlyDictionary<string, string> Build()
    {
        Dictionary<string, string> map = new Dictionary<string, string>();
        foreach (Verse.ModContentPack mcp in Verse.LoadedModManager.RunningMods)
        {
            foreach (Assembly asm in mcp.assemblies.loadedAssemblies)
            {
                string? name = asm.GetName().Name;
                if (name != null) map[name] = mcp.Name;
            }
        }
        return map;
    }
}
