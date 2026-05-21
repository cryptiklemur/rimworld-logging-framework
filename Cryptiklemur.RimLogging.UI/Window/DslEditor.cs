using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using Cryptiklemur.RimLogging.Filtering;
using Cryptiklemur.RimLogging.UI.Filter;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cryptiklemur.RimLogging.UI.Window;

internal sealed class DslEditor
{
    private readonly ChipFilterState _state;
    private string? _lastError;

    public DslEditor(ChipFilterState state)
    {
        _state = state;
    }

    public string? LastError => _lastError;

    public LightweaveNode Build()
    {
        LightweaveNode textField = TextField.Create(
            value: _state.DslSource,
            onChange: text =>
            {
                _state.DslSource = text;
                FilterExpression.TryParse(text, out _, out _lastError);
            },
            placeholder: "Enter filter expression...",
            id: "filter-dsl-editor"
        );

        // v1: error shown as label. Per-character underline requires Lightweave Decoration support.
        if (_lastError == null)
            return textField;

        return Column.Create(
            gap: new Rem(0.25f),
            children: col =>
            {
                col.Add(textField);
                col.Add(Text.Create(
                    _lastError,
                    style: new Style { TextColor = ThemeSlot.StatusDanger }
                ));
            }
        );
    }
}
