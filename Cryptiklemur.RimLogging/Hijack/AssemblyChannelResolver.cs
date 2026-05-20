using System.Reflection;
using Cryptiklemur.RimLogging.Capture;

namespace Cryptiklemur.RimLogging.Hijack
{
    internal static class AssemblyChannelResolver
    {
        /// <summary>
        /// Verse-aware resolver — looks up the supplied assembly in
        /// <see cref="Verse.LoadedModManager.RunningMods"/> and returns
        /// <c>"Mod.&lt;sanitized-packageId&gt;"</c> when matched,
        /// <see cref="AssemblyChannelCache.Unknown"/> otherwise.
        /// </summary>
        public static string Resolve(Assembly asm)
        {
            foreach (Verse.ModContentPack mcp in Verse.LoadedModManager.RunningMods)
            {
                for (int i = 0; i < mcp.assemblies.loadedAssemblies.Count; i++)
                {
                    if (ReferenceEquals(mcp.assemblies.loadedAssemblies[i], asm))
                        return "Mod." + PackageIdSanitizer.ToChannelSegment(mcp.PackageId);
                }
            }
            return AssemblyChannelCache.Unknown;
        }
    }
}
