using Cryptiklemur.RimLogging.Capture;
using Xunit;

namespace RimLoggingTestsExternal.Capture;

public class StackWalkerTests
{
    [Fact]
    public void WalkOnce_FromTest_ReturnsTestMethodName()
    {
        SourceLocation loc = StackWalker.WalkOnce();

        Assert.Equal(nameof(WalkOnce_FromTest_ReturnsTestMethodName), loc.Method);
        Assert.True(loc.IsCallerProvided);
    }

    [Fact]
    public void WalkOnce_FromFrameworkHelper_SkipsFrameworkFrameAndReturnsOuterCaller()
    {
        SourceLocation loc = TestStackWalkerHelper.CallWalker();

        Assert.Equal(
            nameof(WalkOnce_FromFrameworkHelper_SkipsFrameworkFrameAndReturnsOuterCaller),
            loc.Method);
        Assert.True(loc.IsCallerProvided);
    }

    [Fact]
    public void NormalizePath_WindowsModsPath_ProducesShortPath()
    {
        string result = StackWalker.NormalizePath(@"C:\Games\RimWorld\Mods\MyMod\Foo.cs");

        Assert.Equal(@"MyMod\Foo", result);
    }

    [Fact]
    public void NormalizePath_UnixModsPath_ProducesShortPath()
    {
        string result = StackWalker.NormalizePath("/home/x/RimWorld/Mods/MyMod/Foo.cs");

        Assert.Equal("MyMod/Foo", result);
    }

    [Fact]
    public void NormalizePath_RimworldCosmereDoubleDir_Collapses()
    {
        string result = StackWalker.NormalizePath("/home/x/RimworldCosmere/RimworldCosmere/SomeMod/Bar.cs");

        Assert.Equal("SomeMod/Bar", result);
    }

    [Fact]
    public void NormalizePath_DupSiblingDir_CollapsesOnceMore()
    {
        string result = StackWalker.NormalizePath(@"C:\X\RimWorld\Mods\MyMod\MyMod\file.cs");

        Assert.Equal(@"MyMod\file", result);
    }
}
