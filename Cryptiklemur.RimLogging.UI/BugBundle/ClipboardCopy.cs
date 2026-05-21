namespace Cryptiklemur.RimLogging.UI.BugBundle;

internal static class ClipboardCopy
{
    public static void Set(string text)
    {
        UnityEngine.GUIUtility.systemCopyBuffer = text ?? string.Empty;
    }
}
