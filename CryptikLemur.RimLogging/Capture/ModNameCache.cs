using System;
using System.Collections.Generic;
using System.Reflection;

namespace CryptikLemur.RimLogging.Capture;

/// <summary>
/// Caches the assembly-simple-name to mod-name (About.xml <c>&lt;name&gt;</c>) map. Bootstrap
/// installs the Verse-aware <see cref="Provider"/>; the map is built once on first non-empty
/// result. The emit pipeline uses it to attribute log entries to their originating mod without
/// a per-call stack walk.
/// </summary>
internal static class ModNameCache
{
    private static readonly IReadOnlyDictionary<string, string> Empty = new Dictionary<string, string>();

    /// <summary>
    /// Provider hook. Bootstrap sets the Verse-aware implementation; tests set it directly.
    /// When <c>null</c>, the cache yields an empty map.
    /// </summary>
    internal static Func<IReadOnlyDictionary<string, string>>? Provider;

    private static IReadOnlyDictionary<string, string>? _cached;

    /// <summary>
    /// Returns the assembly-name to mod-name map, building it via <see cref="Provider"/> and
    /// caching the first non-empty result. Empty results are not cached, so a provider invoked
    /// before mods finish loading is retried on the next call.
    /// </summary>
    internal static IReadOnlyDictionary<string, string> Map()
    {
        if (_cached != null) return _cached;
        if (Provider == null) return Empty;
        IReadOnlyDictionary<string, string> map;
        try { map = Provider() ?? Empty; }
        catch { return Empty; }
        if (map.Count > 0) _cached = map;
        return map;
    }

    /// <summary>Returns the mod name for the given assembly, or <c>null</c> when unknown.</summary>
    internal static string? ForAssembly(Assembly asm)
    {
        string? name = asm.GetName().Name;
        if (name == null) return null;
        return Map().TryGetValue(name, out string? mod) ? mod : null;
    }

    internal static void ClearForTests()
    {
        _cached = null;
        Provider = null;
    }
}
