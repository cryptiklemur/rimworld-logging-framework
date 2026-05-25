using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CryptikLemur.RimLogging.Settings;

/// <summary>Draws the RimWorld mod settings UI for the logging framework, editing the settings in place.</summary>
public static class LoggingSettingsWindow
{
    /// <summary>Renders controls for global min level, log directory, retention count, proxy URL, and a reset button, mutating <paramref name="s"/> directly.</summary>
    /// <param name="s">The settings instance to display and edit.</param>
    /// <param name="rect">The rect to draw the UI within.</param>
    public static void Render(LoggingSettings s, Rect rect)
    {
        Listing_Standard l = new();
        l.Begin(rect);

        l.Label("CRL_Settings_GlobalMinLevel".Translate() + ": " + s.globalMinLevel);
        if (l.ButtonText(s.globalMinLevel.ToString()))
        {
            List<FloatMenuOption> opts = new();
            foreach (LogLevel lv in Enum.GetValues(typeof(LogLevel)))
                opts.Add(new FloatMenuOption(lv.ToString(), () => s.globalMinLevel = lv));
            Find.WindowStack.Add(new FloatMenu(opts));
        }

        l.Gap();
        l.Label("CRL_Settings_LogDir".Translate());
        s.logDirectory = l.TextEntry(s.logDirectory);

        l.Gap();
        l.Label("CRL_Settings_Retention".Translate() + ": " + s.retentionCount);
        s.retentionCount = (int)l.Slider(s.retentionCount, 1, 50);

        l.Gap();
        l.Label("CRL_Settings_ProxyUrl".Translate());
        s.proxyUrl = l.TextEntry(s.proxyUrl);

        l.Gap();
        l.CheckboxLabeled("CRL_Settings_CaptureStackTraces".Translate(), ref s.captureStackTraces);

        l.Gap();
        if (l.ButtonText("CRL_Settings_Reset".Translate()))
        {
            s.globalMinLevel = LoggingSettingsDefaults.GlobalMinLevel;
            s.logDirectory = LogDirectory.Default;
            s.retentionCount = LoggingSettingsDefaults.RetentionCount;
            s.proxyUrl = LoggingSettingsDefaults.ProxyUrl;
            s.captureStackTraces = LoggingSettingsDefaults.CaptureStackTraces;
        }

        l.End();
    }
}
