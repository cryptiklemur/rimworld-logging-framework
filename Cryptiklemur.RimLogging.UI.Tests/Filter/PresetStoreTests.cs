using System.Collections.Generic;
using Cryptiklemur.RimLogging.UI.Filter;
using Xunit;

namespace Cryptiklemur.RimLogging.UI.Tests.Filter;

public class PresetStoreTests
{
    private sealed class InMemoryStorage : IPresetStorage
    {
        public List<string> Names { get; } = new List<string>();
        public List<string> Expressions { get; } = new List<string>();
        public int PersistCallCount;
        public void Persist() => PersistCallCount++;
    }

    [Fact]
    public void List_EmptyStorage_YieldsNothing()
    {
        InMemoryStorage s = new InMemoryStorage();
        PresetStore store = new PresetStore(s);
        Assert.Empty(store.List());
    }

    [Fact]
    public void Add_AppendsAndPersists()
    {
        InMemoryStorage s = new InMemoryStorage();
        PresetStore store = new PresetStore(s);
        store.Add("noisy mods", "channel = \"Cosmere.*\"");
        Assert.Single(s.Names);
        Assert.Equal("noisy mods", s.Names[0]);
        Assert.Equal("channel = \"Cosmere.*\"", s.Expressions[0]);
        Assert.Equal(1, s.PersistCallCount);
    }

    [Fact]
    public void Remove_KnownName_StripsBothListsAndReturnsTrue()
    {
        InMemoryStorage s = new InMemoryStorage();
        s.Names.AddRange(new[] { "a", "b" });
        s.Expressions.AddRange(new[] { "ax", "bx" });
        PresetStore store = new PresetStore(s);
        bool removed = store.Remove("a");
        Assert.True(removed);
        Assert.Single(s.Names);
        Assert.Equal("b", s.Names[0]);
        Assert.Equal("bx", s.Expressions[0]);
        Assert.Equal(1, s.PersistCallCount);
    }

    [Fact]
    public void Remove_UnknownName_ReturnsFalseAndDoesNotPersist()
    {
        InMemoryStorage s = new InMemoryStorage();
        s.Names.Add("only");
        s.Expressions.Add("only-expr");
        PresetStore store = new PresetStore(s);
        bool removed = store.Remove("absent");
        Assert.False(removed);
        Assert.Single(s.Names);
        Assert.Equal(0, s.PersistCallCount);
    }

    [Fact]
    public void List_ReturnsPairsInOrder()
    {
        InMemoryStorage s = new InMemoryStorage();
        s.Names.AddRange(new[] { "x", "y", "z" });
        s.Expressions.AddRange(new[] { "xe", "ye", "ze" });
        PresetStore store = new PresetStore(s);
        (string, string)[] pairs = System.Linq.Enumerable.ToArray(store.List());
        Assert.Equal(3, pairs.Length);
        Assert.Equal(("x", "xe"), pairs[0]);
        Assert.Equal(("z", "ze"), pairs[2]);
    }

    [Fact]
    public void List_MismatchedLengths_YieldsUpToShorter()
    {
        InMemoryStorage s = new InMemoryStorage();
        s.Names.AddRange(new[] { "a", "b", "c" });
        s.Expressions.AddRange(new[] { "ax" });
        PresetStore store = new PresetStore(s);
        (string, string)[] pairs = System.Linq.Enumerable.ToArray(store.List());
        Assert.Single(pairs);
        Assert.Equal(("a", "ax"), pairs[0]);
    }
}
