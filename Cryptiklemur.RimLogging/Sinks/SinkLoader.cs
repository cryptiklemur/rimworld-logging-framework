using System.Collections.Generic;

namespace Cryptiklemur.RimLogging.Sinks;

internal static class SinkLoader
{
    /// <summary>
    /// Iterates DefDatabase&lt;SinkDef&gt;.AllDefs, instantiates each enabled sink via reflection,
    /// and registers it with Logging. Unknown types or constructor failures are logged via
    /// Verse.Log.Warning and skipped.
    /// </summary>
    public static void LoadDefaults() => LoadFrom(Verse.DefDatabase<SinkDef>.AllDefs);

    internal static void LoadFrom(IEnumerable<SinkDef> defs)
    {
        foreach (SinkDef def in defs)
        {
            if (!def.enabledByDefault) continue;
            ILogSink? sink = TryCreate(def);
            if (sink == null) continue;
            Logging.RegisterSink(sink);
        }
    }

    private static ILogSink? TryCreate(SinkDef def)
    {
        try
        {
            System.Type? type = System.Type.GetType(def.sinkClass, throwOnError: false);
            if (type == null)
            {
                Verse.Log.Warning($"[RimLogging] SinkDef '{def.defName}' references unknown type '{def.sinkClass}', skipping.");
                return null;
            }

            if (type == typeof(RollingTextFileSink) || type == typeof(RollingJsonFileSink))
            {
                Settings.LoggingSettings s = Settings.LoggingMod.Settings;
                return (ILogSink?)System.Activator.CreateInstance(type, s.logDirectory, s.retentionCount, def.minLevel);
            }

            if (type == typeof(VerseLogSink))
            {
                return (ILogSink?)System.Activator.CreateInstance(type, def.minLevel);
            }

            // Unknown but instantiable ILogSink — try parameterless ctor.
            return (ILogSink?)System.Activator.CreateInstance(type);
        }
        catch (System.Exception ex)
        {
            Verse.Log.Warning($"[RimLogging] SinkDef '{def.defName}' failed to instantiate: {ex.Message}");
            return null;
        }
    }
}
