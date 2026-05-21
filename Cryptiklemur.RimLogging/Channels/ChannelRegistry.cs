using System;
using System.Collections.Generic;
using System.Linq;

namespace Cryptiklemur.RimLogging.Channels;

public static class ChannelRegistry
{
    private static Dictionary<string, ChannelDef>? _byName;

    internal static void Boot()
    {
        Dictionary<string, ChannelDef> table = new Dictionary<string, ChannelDef>(StringComparer.Ordinal);
        foreach (ChannelDef d in Verse.DefDatabase<ChannelDef>.AllDefs)
            table[d.defName] = d;
        _byName = table;
    }

    public static ChannelDef? TryResolve(string channelName)
    {
        if (_byName == null) return null;
        string? key = ChannelResolution.ResolveOwnerKey(channelName, _byName.Keys);
        return key != null && _byName.TryGetValue(key, out ChannelDef? def) ? def : null;
    }

    public static IReadOnlyList<ChannelDef> AllRegisteredDefs =>
        _byName == null ? [] : _byName.Values.ToList();
}
