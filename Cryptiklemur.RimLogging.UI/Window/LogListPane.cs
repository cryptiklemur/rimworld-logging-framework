using System;
using System.Collections.Generic;
using Cosmere.Lightweave.Hooks;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using Text = Cosmere.Lightweave.Typography.Typography.Text;
using Cryptiklemur.RimLogging;
using Cryptiklemur.RimLogging.Format;
using Cryptiklemur.RimLogging.UI;
using UnityEngine;

namespace Cryptiklemur.RimLogging.UI.Window;

internal sealed class LogListPane
{
    private readonly UISink _sink;
    private readonly ChannelTreePane _channelTree;
    private readonly LightweaveScrollStatus _scrollStatus = new LightweaveScrollStatus();

    public LogListPane(UISink sink, ChannelTreePane channelTree)
    {
        _sink = sink;
        _channelTree = channelTree;
    }

    public LightweaveNode Build()
    {
        // v1: always tails. Resume-tail UX deferred to Lightweave overlay primitives.
        _scrollStatus.Position = new Vector2(
            _scrollStatus.Position.x,
            Math.Max(0f, _scrollStatus.LastContentHeight - _scrollStatus.LastViewportHeight)
        );

        IReadOnlyList<LogEntry> snapshot = _sink.Snapshot();

        List<LogEntry> visible = new List<LogEntry>(snapshot.Count);
        foreach (LogEntry entry in snapshot)
        {
            if (_channelTree.IsEnabled(entry.Channel))
            {
                visible.Add(entry);
            }
        }

        LightweaveNode rows = Each.Of(
            visible,
            (LogEntry entry, int _) => BuildRow(entry),
            orientation: EachOrientation.Vertical,
            gap: new Rem(0f)
        );

        return ScrollArea.External(rows, _scrollStatus);
    }

    private static LightweaveNode BuildRow(LogEntry entry)
    {
        string timestamp = entry.Timestamp.ToString("HH:mm:ss.fff");
        string levelLabel = entry.Level.ToString().ToUpperInvariant();
        Color levelColor = LevelToColor(entry.Level);
        string source = entry.Source.IsCallerProvided
            ? $"{entry.Source.File}:{entry.Source.Line}"
            : string.Empty;

        return HStack.Create(
            gap: new Rem(0.375f),
            children: row =>
            {
                row.Add(
                    Text.Create(
                        timestamp,
                        style: new Style { TextColor = ThemeSlot.TextMuted, FontSize = new Rem(0.75f) }
                    ),
                    72f
                );
                row.Add(
                    Text.Create(
                        levelLabel,
                        style: new Style { TextColor = (ColorRef)levelColor, FontSize = new Rem(0.75f) }
                    ),
                    44f
                );
                row.Add(
                    Text.Create(
                        entry.Channel,
                        style: new Style { TextColor = ThemeSlot.TextSecondary, FontSize = new Rem(0.75f) }
                    ),
                    96f
                );
                row.Add(
                    Text.Create(
                        source,
                        style: new Style { TextColor = ThemeSlot.TextMuted, FontSize = new Rem(0.7f) }
                    ),
                    140f
                );
                row.AddFlex(
                    Text.Create(
                        entry.RenderedMessage,
                        wrap: false,
                        style: new Style { TextColor = ThemeSlot.TextPrimary, FontSize = new Rem(0.8125f) }
                    )
                );
            }
        );
    }

    private static Color LevelToColor(LogLevel level)
    {
        string hex = SeverityColors.GetHex(level);
        Color color;
        ColorUtility.TryParseHtmlString("#" + hex, out color);
        return color;
    }
}
