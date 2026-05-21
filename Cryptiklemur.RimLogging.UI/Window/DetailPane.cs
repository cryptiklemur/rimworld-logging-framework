using System.Collections.Generic;
using System.Text;
using Cosmere.Lightweave.Data;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using Cryptiklemur.RimLogging;
using Cryptiklemur.RimLogging.Format;
using UnityEngine;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cryptiklemur.RimLogging.UI.Window;

internal sealed class DetailPane
{
    private readonly SelectionStore _selection;

    public DetailPane(SelectionStore selection)
    {
        _selection = selection;
    }

    public LightweaveNode Build()
    {
        LogEntry? e = _selection.Selected;
        if (e == null)
        {
            return Text.Create(
                "No entry selected",
                style: new Style { TextColor = ThemeSlot.TextMuted }
            );
        }

        return Column.Create(children: cols =>
        {
            // Timestamp
            cols.Add(Text.Create(
                e.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                style: new Style { TextColor = ThemeSlot.TextMuted, FontSize = new Rem(0.75f) }
            ));

            // Level + channel row
            string levelHex = SeverityColors.GetHex(e.Level);
            Color levelColor;
            ColorUtility.TryParseHtmlString("#" + levelHex, out levelColor);

            cols.Add(HStack.Create(
                gap: new Rem(0.5f),
                children: row =>
                {
                    row.AddHug(Text.Create(
                        e.Level.ToString().ToUpperInvariant(),
                        style: new Style { TextColor = (ColorRef)levelColor, FontSize = new Rem(0.75f) }
                    ));
                    row.AddHug(Text.Create(
                        e.Channel,
                        style: new Style { TextColor = ThemeSlot.TextSecondary, FontSize = new Rem(0.75f) }
                    ));
                }
            ));

            // Source location
            string sourceText = e.Source.IsCallerProvided
                ? $"{e.Source.File}:{e.Source.Line}"
                : "(no source)";
            cols.Add(Text.Create(
                sourceText,
                style: new Style { TextColor = ThemeSlot.TextMuted, FontSize = new Rem(0.7f) }
            ));

            // Message
            cols.Add(Text.Create(
                e.RenderedMessage,
                wrap: true,
                style: new Style { TextColor = ThemeSlot.TextPrimary, FontSize = new Rem(0.8125f) }
            ));

            // Structured context
            if (e.Context != null && e.Context.Count > 0)
            {
                foreach (KeyValuePair<string, object?> kv in e.Context)
                {
                    string valueText = kv.Value?.ToString() ?? "null";
                    cols.Add(KeyValue.Create(
                        kv.Key,
                        Text.Create(
                            valueText,
                            style: new Style { TextColor = ThemeSlot.TextPrimary, FontSize = new Rem(0.75f) }
                        )
                    ));
                }
            }

            // Stack trace
            // TODO: add collapse/expand once Lightweave has a Disclosure primitive
            string? traceText = e.StackTrace ?? e.Exception?.ToString();
            if (traceText != null)
            {
                cols.Add(Box.Create(
                    children: kids => kids.Add(Text.Create(
                        traceText,
                        wrap: true,
                        style: new Style { TextColor = ThemeSlot.TextMuted, FontSize = new Rem(0.6875f) }
                    ))
                ));
            }

            // Copy button
            cols.Add(Cosmere.Lightweave.Input.Button.Create(
                "Copy",
                onClick: () =>
                {
                    string blob = DefaultFormat.Render(DefaultFormat.Default, e, stripRichText: true);
                    UnityEngine.GUIUtility.systemCopyBuffer = blob;
                }
            ));
        });
    }
}
