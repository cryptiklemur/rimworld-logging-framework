// Lightweave's Toast primitive is a stateful node factory (Toast.Create takes IReadOnlyList<ToastMessage>
// and an onDismiss callback). Wiring a reactive toast panel into the viewer is a Phase 10 concern.
// For v1 we use RimWorld's vanilla message ticker which is available everywhere in-game.
namespace Cryptiklemur.RimLogging.UI.BugBundle;

internal static class BundleToast
{
    public static void Success(string url)
    {
        Verse.Messages.Message(
            $"Bug report shared. URL copied to clipboard: {url}",
            RimWorld.MessageTypeDefOf.PositiveEvent);
    }

    public static void Failure(string errorMessage)
    {
        Verse.Messages.Message(
            $"Bug report upload failed: {errorMessage}",
            RimWorld.MessageTypeDefOf.RejectInput);
    }
}
