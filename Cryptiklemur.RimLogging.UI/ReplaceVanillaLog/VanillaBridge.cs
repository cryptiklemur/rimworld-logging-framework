namespace Cryptiklemur.RimLogging.UI.ReplaceVanillaLog;

internal static class VanillaBridge
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance", "CA2211",
        Justification = "Cross-thread sentinel set briefly by debug action; volatile read in Harmony prefix.")]
    public static volatile bool ForceVanilla;

    /// <summary>
    /// Opens the vanilla RimWorld debug log window, bypassing our Harmony patch temporarily.
    /// Modders or debug actions can call this to force the vanilla debug log window past our Harmony patch.
    /// </summary>
    public static void OpenVanilla()
    {
        ForceVanilla = true;
        try
        {
            Verse.Log.TryOpenLogWindow();
        }
        finally
        {
            ForceVanilla = false;
        }
    }
}
