// TODO (Phase 10): Wire ShareBundleButton into LogViewerWindow's header right slot.
// Instantiate with the viewer's UISink and the proxy URL from LoggingSettings.
using System.Collections.Generic;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Runtime;
using Cryptiklemur.RimLogging.Bundle;

namespace Cryptiklemur.RimLogging.UI.BugBundle;

internal sealed class ShareBundleButton
{
    private readonly UISink _sink;
    private readonly string _proxyUrl;
    private bool _uploading;

    public ShareBundleButton(UISink sink, string proxyUrl)
    {
        _sink = sink;
        _proxyUrl = proxyUrl;
    }

    public LightweaveNode Build()
    {
        return Button.Create(
            label: _uploading ? "Sharing..." : "Share bundle",
            onClick: _uploading ? null : () => UploadAsync(),
            disabled: _uploading);
    }

    /// <summary>
    /// async void is intentional: this is a fire-and-forget UI event handler.
    /// Unity's game loop has no awaitable entry point for button callbacks, so
    /// returning Task would silently discard the result anyway. Exceptions are
    /// caught internally and surfaced via BundleToast.Failure.
    /// </summary>
    private async void UploadAsync()
    {
        _uploading = true;
        try
        {
            IReadOnlyList<LogEntry> entries = _sink.Snapshot();
            BundlePayload payload = BundlerSessionFactory.BuildForRunningSession(entries);
            ProxyClient proxy = new ProxyClient(_proxyUrl);
            ProxyResult r = await proxy.UploadAsync(payload).ConfigureAwait(false);
            if (r.Success && !string.IsNullOrEmpty(r.GistUrl))
            {
                ClipboardCopy.Set(r.GistUrl!);
                BundleToast.Success(r.GistUrl!);
            }
            else
            {
                BundleToast.Failure(r.ErrorMessage ?? "Upload failed.");
            }
        }
        finally
        {
            _uploading = false;
        }
    }
}
