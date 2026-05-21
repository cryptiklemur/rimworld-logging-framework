using System;
using System.Collections.Generic;
using Cosmere.Lightweave.Data;
using Cosmere.Lightweave.Runtime;
using Cryptiklemur.RimLogging;
using Cryptiklemur.RimLogging.Filters;
using Cryptiklemur.RimLogging.UI;

namespace Cryptiklemur.RimLogging.UI.Window;

internal sealed class ChannelTreePane
{
    private readonly UISink _sink;
    private readonly Dictionary<string, bool> _enabled = new Dictionary<string, bool>();

    public ChannelTreePane(UISink sink)
    {
        _sink = sink;
    }

    public IReadOnlyDictionary<string, bool> ChannelEnabled => _enabled;

    public bool IsEnabled(string channel) => !_enabled.TryGetValue(channel, out bool v) || v;

    public LightweaveNode Build()
    {
        IReadOnlyList<LogEntry> snapshot = _sink.Snapshot();

        // Lightweave Tree has no per-row content slot -- toggle state shown in label prefix
        List<TreeNode> roots = new List<TreeNode>();

        // Registered defs grouped by ownerMod (Phase 10 will populate AllRegisteredDefs and add Name/OwnerMod to ChannelDef)
        // AllRegisteredDefs returns empty until Phase 10 -- this block renders zero children at v1.0.0-beta
        IReadOnlyList<ChannelDef> defs = ChannelRegistry.AllRegisteredDefs;
        if (defs.Count > 0)
        {
            // Phase 10: iterate defs, group by def.OwnerMod ?? "Unregistered", build TreeNode rows
            roots.Add(new TreeNode("Registered", null, null));
        }

        // Transient group: channels seen in snapshots with no registered def
        List<TreeNode> transient = new List<TreeNode>();
        HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (LogEntry entry in snapshot)
        {
            string channel = entry.Channel;
            if (!seen.Add(channel)) continue;
            if (ChannelRegistry.TryResolve(channel) != null) continue;
            string prefix = IsEnabled(channel) ? "[+] " : "[-] ";
            transient.Add(new TreeNode(prefix + channel, null, channel));
        }
        if (transient.Count > 0)
        {
            roots.Add(new TreeNode("Transient", transient, null));
        }

        Action<TreeNode> onSelect = node => {
            if (node.Payload is string channelName)
            {
                bool current = IsEnabled(channelName);
                _enabled[channelName] = !current;
            }
        };

        return Tree.Create(roots, onSelect);
    }
}
