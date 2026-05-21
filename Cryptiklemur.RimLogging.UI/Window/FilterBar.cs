using System;
using System.Collections.Generic;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using Cryptiklemur.RimLogging;
using Cryptiklemur.RimLogging.Format;
using Cryptiklemur.RimLogging.UI.Filter;
using UnityEngine;

namespace Cryptiklemur.RimLogging.UI.Window;

internal sealed class FilterBar
{
    private static readonly string[] LevelLabels = { "TRC", "DBG", "INF", "WRN", "ERR", "FTL" };
    private static readonly LogLevel[] Levels =
    {
        LogLevel.Trace, LogLevel.Debug, LogLevel.Info,
        LogLevel.Warn,  LogLevel.Error, LogLevel.Fatal,
    };

    private readonly ChipFilterState _state;
    private readonly IReadOnlyList<string> _presetNames;

    public FilterBar(ChipFilterState state, IReadOnlyList<string> presetNames)
    {
        _state = state;
        _presetNames = presetNames;
    }

    public LightweaveNode Build()
    {
        return HStack.Create(
            gap: new Rem(0.375f),
            children: row =>
            {
                // Search field
                row.AddHug(SearchField.Create(
                    value: _state.SearchText,
                    onChange: text => _state.SearchText = text,
                    placeholder: "Search...",
                    id: "filter-search"
                ));

                // Level chips (hidden in DSL mode)
                if (!_state.DslMode)
                {
                    for (int i = 0; i < Levels.Length; i++)
                    {
                        int idx = i;
                        LogLevel level = Levels[idx];
                        bool active = _state.Levels[idx];

                        Color chipColor;
                        ColorUtility.TryParseHtmlString("#" + SeverityColors.GetHex(level), out chipColor);
                        Color textColor = active
                            ? chipColor
                            : new Color(chipColor.r, chipColor.g, chipColor.b, 0.35f);

                        row.AddHug(Button.Create(
                            LevelLabels[idx],
                            onClick: () => _state.Levels[idx] = !_state.Levels[idx],
                            variant: active ? Variant.Secondary : Variant.Ghost,
                            style: new Style { TextColor = (ColorRef)textColor },
                            id: "filter-chip-" + LevelLabels[idx]
                        ));
                    }
                }

                // Preset dropdown
                // TODO: wire full settings integration when LoggingMod.Settings is accessible here
                if (_presetNames.Count > 0)
                {
                    string selected = _presetNames[0];
                    row.AddHug(Dropdown.Create(
                        value: selected,
                        options: _presetNames,
                        labelFn: s => s,
                        onChange: _ => { },
                        instanceKey: "filter-presets"
                    ));
                }
                else
                {
                    // TODO: replace with real preset dropdown once settings integration lands
                    row.AddHug(Button.Create(
                        "Presets",
                        onClick: null,
                        variant: Variant.Ghost,
                        disabled: true,
                        id: "filter-presets-stub"
                    ));
                }

                // DSL toggle
                row.AddHug(Button.Create(
                    "DSL",
                    onClick: () => _state.DslMode = !_state.DslMode,
                    variant: _state.DslMode ? Variant.Primary : Variant.Ghost,
                    id: "filter-dsl-toggle"
                ));

                // DSL editor (visible only in DSL mode)
                if (_state.DslMode)
                {
                    // Task 8.9 wires this through FilterExpression.TryParse and renders parse errors.
                    row.AddFlex(TextField.Create(
                        value: _state.DslSource,
                        onChange: text => _state.DslSource = text,
                        placeholder: "Enter filter expression...",
                        id: "filter-dsl-editor"
                    ));
                }
            }
        );
    }
}
