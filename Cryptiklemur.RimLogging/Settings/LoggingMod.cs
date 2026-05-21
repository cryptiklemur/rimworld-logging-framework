using Cryptiklemur.RimLogging;
using UnityEngine;
using Verse;

namespace Cryptiklemur.RimLogging.Settings;

public sealed class LoggingMod : Mod
{
    public static LoggingSettings Settings { get; private set; } = null!;

    public LoggingMod(ModContentPack content) : base(content)
    {
        Settings = GetSettings<LoggingSettings>();
        Logging.GlobalMinLevel = Settings.globalMinLevel;
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        LoggingSettingsWindow.Render(Settings, inRect);
        Logging.GlobalMinLevel = Settings.globalMinLevel;
    }

    public override string SettingsCategory() => "RimLogging";
}
