namespace CryptikLemur.RimLogging.Capture;

/// <summary>
/// Test-only helper that lives under the framework namespace so it is skipped
/// by <see cref="StackWalker.WalkOnce"/>'s frame-skip logic, allowing tests to
/// verify the outer-caller frame is returned.
/// </summary>
internal static class TestStackWalkerHelper
{
    public static SourceLocation CallWalker() => StackWalker.WalkOnce();

    /// <summary>
    /// Captures a stack from inside the framework namespace and runs it through
    /// <see cref="StackWalker.FirstCallerFrame"/> so tests can verify that this
    /// helper's frame is skipped and the outer test caller is returned.
    /// </summary>
    public static SourceLocation CallFirstCallerFrame()
    {
        System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(0, true);
        return StackWalker.FirstCallerFrame(st);
    }


    /// <summary>
    /// Cheap counterpart of <see cref="CallFirstCallerFrame"/> that exercises the
    /// no-PDB walk used by <c>Log.ResolveSource</c> for <c>[CallerFilePath]</c> paths.
    /// </summary>
    public static System.Type? CallFirstCallerType()
    {
        System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(0, false);
        return StackWalker.FirstCallerType(st);
    }
}
