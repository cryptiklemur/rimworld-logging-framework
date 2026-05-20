using System;

namespace Cryptiklemur.RimLogging.Capture;

/// <summary>
/// Runtime fallback that resolves the originating <see cref="SourceLocation"/> by
/// walking the managed stack when compile-time caller attributes are unavailable.
/// </summary>
public static class StackWalker
{
    private static readonly System.Text.RegularExpressions.Regex _pathStrip = new(
        @"^.*?(RimworldCosmere[\\/]RimworldCosmere[\\/]|RimWorld[\\/]Mods[\\/])+[\\/]*",
        System.Text.RegularExpressions.RegexOptions.Compiled);
    private static readonly System.Text.RegularExpressions.Regex _dupDir = new(
        @"^(\w+)[\\/]\1[\\/]",
        System.Text.RegularExpressions.RegexOptions.Compiled);

    /// <summary>
    /// Walks the current stack and returns the first frame outside the
    /// <c>Cryptiklemur.RimLogging.</c> namespace, normalising its file path so the
    /// resulting <see cref="SourceLocation"/> is stable across machines and layouts.
    /// </summary>
    /// <returns>
    /// A populated <see cref="SourceLocation"/> when a usable frame is found, or
    /// <see cref="SourceLocation.Empty"/> when no caller frame carries file info.
    /// </returns>
    public static SourceLocation WalkOnce()
    {
        System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(1, true);
        for (int i = 0; i < st.FrameCount; i++)
        {
            System.Diagnostics.StackFrame? frame = st.GetFrame(i);
            System.Reflection.MethodBase? method = frame?.GetMethod();
            string? declaring = method?.DeclaringType?.FullName;
            if (declaring != null && declaring.StartsWith("Cryptiklemur.RimLogging.", StringComparison.Ordinal))
                continue;
            string? file = frame?.GetFileName();
            if (file == null) return SourceLocation.Empty;
            string clean = NormalizePath(file);
            return new SourceLocation(clean, frame!.GetFileLineNumber(), method?.Name);
        }
        return SourceLocation.Empty;
    }

    /// <summary>
    /// Normalises an absolute source-file path into a short, stable form by
    /// stripping common RimWorld layout prefixes, collapsing duplicated sibling
    /// directories, and removing the trailing <c>.cs</c> extension.
    /// </summary>
    /// <param name="file">Raw file path from <c>StackFrame.GetFileName()</c>.</param>
    /// <returns>Short normalised path, never <c>null</c>.</returns>
    internal static string NormalizePath(string file)
    {
        string clean = _pathStrip.Replace(file, string.Empty);
        clean = _dupDir.Replace(clean, "$1\\");
        clean = clean.TrimStart('\\', '/').Replace(".cs", string.Empty);
        return clean;
    }
}
