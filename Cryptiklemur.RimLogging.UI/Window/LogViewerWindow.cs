using System;
using Cosmere.Lightweave.Layout;
using Cryptiklemur.RimLogging.UI.Filter;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;

namespace Cryptiklemur.RimLogging.UI.Window;

internal sealed class LogViewerWindow : LightweaveWindow {
    private readonly UISink _sink;
    private readonly ChannelTreePane _channelTreePane;
    private readonly SelectionStore _selection;
    private readonly LogListPane _logListPane;
    private readonly DetailPane _detailPane;
    private readonly ChipFilterState _filterState;
    private readonly FilterBar _filterBar;

    public LogViewerWindow(UISink sink) {
        _sink = sink;
        _channelTreePane = new ChannelTreePane(sink);
        _selection = new SelectionStore();
        _logListPane = new LogListPane(sink, _channelTreePane, _selection);
        _detailPane = new DetailPane(_selection);
        _filterState = new ChipFilterState();
        _filterBar = new FilterBar(_filterState, Array.Empty<string>());
    }

    protected override float WidthFraction => 0.85f;

    protected override float HeightFraction => 0.7f;

    protected override LightweaveNode? Header() {
        return WindowHeader.Create(title: "RimLogging - Log Viewer");
    }

    protected override LightweaveNode Body() {
        LightweaveNode centerColumn = Column.Create(children: cols => {
            cols.Add(_filterBar.Build());
            cols.Add(_logListPane.Build());
        });

        return HStack.Create(children: row => {
            row.Add(_channelTreePane.Build(), 280f);
            row.AddFlex(centerColumn);
            row.Add(_detailPane.Build(), 360f);
        });
    }
}
