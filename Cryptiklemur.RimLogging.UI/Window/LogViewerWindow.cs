using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;

namespace Cryptiklemur.RimLogging.UI.Window;

internal sealed class LogViewerWindow : LightweaveWindow {
    private readonly UISink _sink;
    private readonly ChannelTreePane _channelTreePane;
    private readonly LogListPane _logListPane;

    public LogViewerWindow(UISink sink) {
        _sink = sink;
        _channelTreePane = new ChannelTreePane(sink);
        _logListPane = new LogListPane(sink, _channelTreePane);
    }

    protected override float WidthFraction => 0.85f;

    protected override float HeightFraction => 0.7f;

    protected override LightweaveNode? Header() {
        return WindowHeader.Create(title: "RimLogging - Log Viewer");
    }

    protected override LightweaveNode Body() {
        LightweaveNode detailPane = Box.Create(id: "detail-pane-placeholder");

        LightweaveNode centerColumn = Column.Create(children: cols => {
            cols.Add(Box.Create(id: "filter-bar-placeholder"));
            cols.Add(_logListPane.Build());
        });

        return HStack.Create(children: row => {
            row.Add(_channelTreePane.Build(), 280f);
            row.AddFlex(centerColumn);
            row.Add(detailPane, 360f);
        });
    }
}
