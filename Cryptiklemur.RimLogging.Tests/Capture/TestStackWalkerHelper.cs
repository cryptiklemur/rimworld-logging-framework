namespace Cryptiklemur.RimLogging.Capture;

/// <summary>
/// Test-only helper that lives under the framework namespace so it is skipped
/// by <see cref="StackWalker.WalkOnce"/>'s frame-skip logic, allowing tests to
/// verify the outer-caller frame is returned.
/// </summary>
internal static class TestStackWalkerHelper
{
    public static SourceLocation CallWalker() => StackWalker.WalkOnce();
}
