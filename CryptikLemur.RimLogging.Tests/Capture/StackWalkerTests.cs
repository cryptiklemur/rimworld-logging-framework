using CryptikLemur.RimLogging.Capture;
using Xunit;

// Namespace outside CryptikLemur.RimLogging.* so this test's frame is not skipped by WalkOnce.
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

    // Regression: NormalizePath used to call Replace(".cs", "") which strips the substring
    // anywhere in the path, not just the trailing extension. A file whose name contains ".cs"
    // mid-string (e.g. a Razor-style "View.cshtml.cs") was mangled to "Viewhtml". Only the
    // trailing ".cs" extension must be removed.
    [Fact]
    public void NormalizePath_DotCsMidString_OnlyStripsTrailingExtension()
    {
        string result = StackWalker.NormalizePath(@"C:\X\RimWorld\Mods\MyMod\View.cshtml.cs");

        Assert.Equal(@"MyMod\View.cshtml", result);
    }

    [Fact]
    public void FirstCallerFrame_FromHelperInFrameworkNamespace_SkipsHelperAndReturnsTestMethod()
    {
        SourceLocation loc = TestStackWalkerHelper.CallFirstCallerFrame();

        Assert.Equal(
            nameof(FirstCallerFrame_FromHelperInFrameworkNamespace_SkipsHelperAndReturnsTestMethod),
            loc.Method);
        Assert.True(loc.IsCallerProvided);
    }

    // Regression: prior FirstCallerFrame bailed on the first non-RimLogging frame whose
    // GetFileName() returned null. In production this meant any vanilla Verse/Unity frame
    // (or the Harmony-emitted dynamic stub) sitting between the framework and the real
    // caller wiped out the Source field. Reflection.Invoke injects similar PDB-less frames
    // (System.RuntimeMethodHandle / RuntimeMethodInfo.UnsafeInvokeInternal) into the
    // stack, so this drives the same scenario: the call chain has frames-without-files
    // between the framework-internal helper and the real test caller. The fix is for
    // FirstCallerFrame to walk past those instead of returning Empty.
    [Fact]
    public void FirstCallerFrame_SkipsReflectionInvokeFramesWithoutFileInfo_AndReturnsRealCaller()
    {
        System.Reflection.MethodInfo m = typeof(TestStackWalkerHelper).GetMethod(
            nameof(TestStackWalkerHelper.CallFirstCallerFrame))!;

        SourceLocation loc = (SourceLocation)m.Invoke(null, null)!;

        Assert.Equal(
            nameof(FirstCallerFrame_SkipsReflectionInvokeFramesWithoutFileInfo_AndReturnsRealCaller),
            loc.Method);
        Assert.True(loc.IsCallerProvided);
    }
}
