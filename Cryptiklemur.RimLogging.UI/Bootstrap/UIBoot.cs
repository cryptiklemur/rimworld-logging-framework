using System;
using HarmonyLib;
using Verse;

namespace Cryptiklemur.RimLogging.UI.Bootstrap;

[StaticConstructorOnStartup]
internal static class UIBoot
{
    static UIBoot()
    {
        if (!LightweaveProbe.IsAvailable())
        {
            Cryptiklemur.RimLogging.Log.Info("Cryptiklemur.RimLogging.UI",
                "Lightweave not detected - RimLogging UI module disabled. Core logging continues to work.");
            return;
        }
        try { Install(); }
        catch (Exception ex)
        {
            Cryptiklemur.RimLogging.Log.Error("Cryptiklemur.RimLogging.UI", ex,
                "Failed to install RimLogging UI module");
        }
    }

    private static void Install()
    {
        Cryptiklemur.RimLogging.Logging.RegisterSink(new UISink());
        Harmony h = new Harmony("cryptiklemur.rimlogging.ui");
        h.PatchAll(typeof(UIBoot).Assembly);
    }
}
