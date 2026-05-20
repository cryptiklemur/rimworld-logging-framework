using System;
using System.Reflection;
using Cryptiklemur.RimLogging.Capture;
using Xunit;

namespace Cryptiklemur.RimLogging.Tests.Capture;

public class AssemblyChannelCacheTests : IDisposable
{
    private static readonly Assembly TestAssembly = typeof(AssemblyChannelCacheTests).Assembly;

    public void Dispose()
    {
        AssemblyChannelCache.ResolverHook = null;
        AssemblyChannelCache.ClearForTests();
    }

    [Theory]
    [InlineData("Assembly-CSharp")]
    [InlineData("Assembly-CSharp-firstpass")]
    [InlineData("UnityEngine")]
    [InlineData("UnityEngine.CoreModule")]
    [InlineData("Verse")]
    public void IsVanillaAssembly_KnownNames_ReturnsTrue(string name)
    {
        Assert.True(AssemblyChannelCache.IsVanillaAssembly(name));
    }

    [Theory]
    [InlineData("MyMod")]
    [InlineData("SomeMod.Core")]
    [InlineData("")]
    public void IsVanillaAssembly_UnknownNames_ReturnsFalse(string name)
    {
        Assert.False(AssemblyChannelCache.IsVanillaAssembly(name));
    }

    [Fact]
    public void Resolve_NoHook_ReturnsUnknown()
    {
        string result = AssemblyChannelCache.Resolve(TestAssembly);
        Assert.Equal(AssemblyChannelCache.Unknown, result);
    }

    [Fact]
    public void Resolve_WithHook_ReturnsHookResult()
    {
        AssemblyChannelCache.ResolverHook = _ => "Mod.fake.test";
        string result = AssemblyChannelCache.Resolve(TestAssembly);
        Assert.Equal("Mod.fake.test", result);
    }

    [Fact]
    public void Resolve_CachesOnFirstCall()
    {
        int callCount = 0;
        AssemblyChannelCache.ResolverHook = _ =>
        {
            callCount++;
            return "Mod.counted." + callCount;
        };

        string first = AssemblyChannelCache.Resolve(TestAssembly);
        string second = AssemblyChannelCache.Resolve(TestAssembly);

        Assert.Equal(first, second);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Resolve_CachedResultSurvivesHookSetToNull()
    {
        AssemblyChannelCache.ResolverHook = _ => "Mod.cached.value";
        string first = AssemblyChannelCache.Resolve(TestAssembly);

        AssemblyChannelCache.ResolverHook = null;
        string second = AssemblyChannelCache.Resolve(TestAssembly);

        Assert.Equal("Mod.cached.value", first);
        Assert.Equal(first, second);
    }

    [Fact]
    public void Resolve_HookThrowing_ReturnsUnknown()
    {
        AssemblyChannelCache.ResolverHook = _ => throw new InvalidOperationException("test failure");
        string result = AssemblyChannelCache.Resolve(TestAssembly);
        Assert.Equal(AssemblyChannelCache.Unknown, result);
    }
}
