using System;
using System.Collections.Generic;
using CryptikLemur.RimLogging.Capture;
using Xunit;

namespace CryptikLemur.RimLogging.Tests.Capture;

public class ModNameCacheTests : IDisposable
{
    public ModNameCacheTests() => ModNameCache.ClearForTests();

    public void Dispose() => ModNameCache.ClearForTests();

    [Fact]
    public void Map_NoProvider_ReturnsEmpty()
    {
        Assert.Empty(ModNameCache.Map());
    }

    [Fact]
    public void Map_CachesFirstNonEmptyResult_ProviderCalledOnce()
    {
        int calls = 0;
        ModNameCache.Provider = () =>
        {
            calls++;
            return new Dictionary<string, string> { ["Asm"] = "Mod" };
        };

        ModNameCache.Map();
        ModNameCache.Map();

        Assert.Equal(1, calls);
    }

    [Fact]
    public void Map_DoesNotCacheEmptyResult_RetriesUntilPopulated()
    {
        int calls = 0;
        ModNameCache.Provider = () =>
        {
            calls++;
            return calls < 2
                ? new Dictionary<string, string>()
                : new Dictionary<string, string> { ["Asm"] = "Mod" };
        };

        Assert.Empty(ModNameCache.Map());
        Assert.Equal("Mod", ModNameCache.Map()["Asm"]);
    }

    [Fact]
    public void Map_ThrowingProvider_ReturnsEmptyAndDoesNotCache()
    {
        ModNameCache.Provider = () => throw new InvalidOperationException("boom");

        Assert.Empty(ModNameCache.Map());
    }
}
