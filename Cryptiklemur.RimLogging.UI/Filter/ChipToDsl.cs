using System.Collections.Generic;
using Cryptiklemur.RimLogging;

namespace Cryptiklemur.RimLogging.UI.Filter;

internal static class ChipToDsl
{
    private static readonly LogLevel[] LevelOrder =
    {
        LogLevel.Trace, LogLevel.Debug, LogLevel.Info,
        LogLevel.Warn,  LogLevel.Error, LogLevel.Fatal,
    };

    public static string Synthesize(ChipFilterState state)
    {
        bool allEnabled = true;
        bool anyEnabled = false;

        for (int i = 0; i < state.Levels.Length; i++)
        {
            if (state.Levels[i])
                anyEnabled = true;
            else
                allEnabled = false;
        }

        // All levels enabled: tautology that matches every entry.
        if (allEnabled)
            return "level >= Trace";

        // No levels enabled: contradiction that matches nothing.
        // Trace is the minimum severity, so level < Trace is always false.
        if (!anyEnabled)
            return "level < Trace";

        List<string> parts = new List<string>();
        for (int i = 0; i < state.Levels.Length; i++)
        {
            if (state.Levels[i])
                parts.Add("level = " + LevelOrder[i]);
        }

        return string.Join(" OR ", parts);
    }
}
