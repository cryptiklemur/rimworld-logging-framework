using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Cryptiklemur.RimLogging.Settings;

public static class LoggingSettingsWindow
{
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
        if (l.ButtonText("CRL_Settings_Reset".Translate()))
        {
            s.globalMinLevel = LogLevel.Info;
            s.logDirectory = LogDirectory.Default;
            s.retentionCount = 5;
            s.proxyUrl = "https://rimlogging-bundle.cryptiklemur.workers.dev/v1/bundle";
        }

        l.End();
    }
}
