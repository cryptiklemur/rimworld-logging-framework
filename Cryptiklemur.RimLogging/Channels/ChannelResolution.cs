using System;
using System.Collections.Generic;

namespace Cryptiklemur.RimLogging.Channels;

/// <summary>Pure dotted-name resolution: exact match, then prefix walk, then "default" fallback.</summary>
public static class ChannelResolution
{
    /// <summary>
    /// Resolves <paramref name="channelName"/> to a registered key. Tries exact match,
    /// then strips trailing dotted segments looking for an ancestor, then falls back to "default".
    /// Returns null if no key matches.
    /// </summary>
    public static string? ResolveOwnerKey(string channelName, IEnumerable<string> registeredKeys)
    {
        if (string.IsNullOrEmpty(channelName)) return null;
        HashSet<string> set = new HashSet<string>(registeredKeys, StringComparer.Ordinal);
        if (set.Contains(channelName)) return channelName;
        string cur = channelName;
        int dot;
        while ((dot = cur.LastIndexOf('.')) > 0)
        {
            cur = cur.Substring(0, dot);
            if (set.Contains(cur)) return cur;
        }
        return set.Contains("default") ? "default" : null;
    }
}
