using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Cryptiklemur.RimLogging.Capture
{
    internal static class AssemblyChannelCache
    {
        private static readonly ConcurrentDictionary<Assembly, string> _cache = new ConcurrentDictionary<Assembly, string>();

        internal const string Vanilla = "Vanilla";
        internal const string Unknown = "Mod.Unknown";

        /// <summary>
        /// Resolver hook. Bootstrap installs the Verse-aware implementation;
        /// tests may set this directly to assert behavior. When null, the cache
        /// returns <see cref="Unknown"/> for non-vanilla assemblies.
        /// </summary>
#pragma warning disable CS0649
        internal static Func<Assembly, string>? ResolverHook;
#pragma warning restore CS0649

        /// <summary>Returns the channel name for the given assembly, using the cache.</summary>
        public static string Resolve(Assembly asm) => _cache.GetOrAdd(asm, ResolveOnce);

        private static string ResolveOnce(Assembly asm)
        {
            string asmName = asm.GetName().Name ?? "Unknown";
            if (IsVanillaAssembly(asmName)) return Vanilla;
            if (ResolverHook != null)
            {
                try { return ResolverHook(asm); }
                catch { return Unknown; }
            }
            return Unknown;
        }

        internal static bool IsVanillaAssembly(string name)
        {
            switch (name)
            {
                case "Assembly-CSharp":
                case "Assembly-CSharp-firstpass":
                case "UnityEngine":
                case "UnityEngine.CoreModule":
                case "Verse":
                    return true;
                default:
                    return false;
            }
        }

        internal static int CacheSize => _cache.Count;

        internal static void ClearForTests() => _cache.Clear();
    }
}
