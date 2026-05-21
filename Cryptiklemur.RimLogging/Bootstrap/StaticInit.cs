namespace Cryptiklemur.RimLogging.Bootstrap;

[Verse.StaticConstructorOnStartup]
internal static class StaticInit
{
    static StaticInit()
    {
        try
        {
            Channels.ChannelRegistry.Boot();
            Logging.Init();
            Logging._isDegradedProvider = () => DegradedMode.IsPresent;
            ApplySettings(Settings.LoggingMod.Settings);
            Sinks.SinkLoader.LoadDefaults();
            if (Hijack.HijackBootstrap.Install())
            {
                Log.Info("Cryptiklemur.RimLogging", "RimLogging initialized");
            }
            else
            {
                Log.Warn("Cryptiklemur.RimLogging",
                    "Another RimLogging instance already installed; running in degraded mode");
            }
        }
        catch (System.Exception ex)
        {
            Verse.Log.Error("[RimLogging] bootstrap failed: " + ex);
        }
    }

    private static void ApplySettings(Settings.LoggingSettings settings)
    {
        Logging.GlobalMinLevel = settings.globalMinLevel;
    }
}
