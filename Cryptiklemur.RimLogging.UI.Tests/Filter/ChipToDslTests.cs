using Cryptiklemur.RimLogging.Filtering;
using Cryptiklemur.RimLogging.UI.Filter;
using Xunit;

namespace Cryptiklemur.RimLogging.UI.Tests.Filter;

public sealed class ChipToDslTests
{
    [Fact]
    public void EmptyState_SynthesizesTautology()
    {
        ChipFilterState state = new ChipFilterState();
        string dsl = ChipToDsl.Synthesize(state);
        Assert.Equal("level >= Trace", dsl);
    }

    [Fact]
    public void DisablingAllLevels_SynthesizesContradiction()
    {
        ChipFilterState state = new ChipFilterState();
        for (int i = 0; i < state.Levels.Length; i++)
            state.Levels[i] = false;
        string dsl = ChipToDsl.Synthesize(state);
        Assert.Equal("level < Trace", dsl);
    }

    [Fact]
    public void TwoLevelsEnabled_SynthesizesOrChain()
    {
        ChipFilterState state = new ChipFilterState();
        for (int i = 0; i < state.Levels.Length; i++)
            state.Levels[i] = false;
        state.Levels[0] = true; // Trace
        state.Levels[3] = true; // Warn
        string dsl = ChipToDsl.Synthesize(state);
        Assert.Equal("level = Trace OR level = Warn", dsl);
    }

    [Fact]
    public void SearchTextNotSynthesized()
    {
        ChipFilterState state = new ChipFilterState();
        state.SearchText = "spawn";
        string dsl = ChipToDsl.Synthesize(state);
        Assert.Equal("level >= Trace", dsl);
    }

    [Fact]
    public void CyclingDslMode_PreservesDslSource()
    {
        ChipFilterState state = new ChipFilterState { DslSource = "channel = \"Cosmere.*\"" };
        state.DslMode = true;
        state.DslMode = false;
        Assert.Equal("channel = \"Cosmere.*\"", state.DslSource);
    }

    [Fact]
    public void Synthesized_RoundtripsThroughParser()
    {
        ChipFilterState state = new ChipFilterState();
        for (int i = 0; i < state.Levels.Length; i++)
            state.Levels[i] = i % 2 == 0;
        string dsl = ChipToDsl.Synthesize(state);

        bool ok = FilterExpression.TryParse(dsl, out FilterExpression? expr, out string? err);
        Assert.True(ok, $"Synthesized DSL failed to parse: '{dsl}' err='{err}'");
        Assert.NotNull(expr);
    }
}
