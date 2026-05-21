using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;

namespace Cryptiklemur.RimLogging.UI.Window;

internal sealed class LogViewerWindow : LightweaveWindow {
    private readonly UISink _sink;

    public LogViewerWindow(UISink sink) {
        _sink = sink;
    }

    protected override float WidthFraction => 0.85f;

    protected override float HeightFraction => 0.7f;

    protected override LightweaveNode? Header() {
        return WindowHeader.Create(title: "RimLogging - Log Viewer");
    }

    protected override LightweaveNode Body() {
        ChannelTreePane channelPane = new ChannelTreePane(_sink);
        LightweaveNode detailPane = Box.Create(id: "detail-pane-placeholder");

        LightweaveNode centerColumn = Column.Create(children: cols => {
            cols.Add(Box.Create(id: "filter-bar-placeholder"));
            cols.Add(Box.Create(id: "log-list-placeholder"));
        });

        return HStack.Create(children: row => {
            row.Add(channelPane.Build(), 280f);
            row.AddFlex(centerColumn);
            row.Add(detailPane, 360f);
        });
    }
}
