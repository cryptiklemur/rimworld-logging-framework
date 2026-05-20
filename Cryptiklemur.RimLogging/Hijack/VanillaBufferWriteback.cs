namespace Cryptiklemur.RimLogging.Hijack
{
    /// <summary>
    /// Phase 5 helper that routes a fully-rendered colored line back into vanilla's Verse.Log buffer.
    /// Internal because callers should go through <c>VerseLogSink</c>.
    /// Lives in Hijack/ so it is excluded from test projects that can't reference Verse.
    /// </summary>
    internal static class VanillaBufferWriteback
    {
        /// <summary>
        /// Writes <paramref name="coloredLine"/> into the appropriate vanilla log method
        /// based on <paramref name="level"/>. If <paramref name="exception"/> is non-null,
        /// its string representation is appended on a new line.
        /// </summary>
        /// <param name="level">Severity level, determines which Verse.Log method is called.</param>
        /// <param name="coloredLine">The fully-rendered, color-tagged line to write.</param>
        /// <param name="exception">Optional exception to append; may be <c>null</c>.</param>
        public static void Write(LogLevel level, string coloredLine, System.Exception? exception)
        {
            string text = exception != null ? coloredLine + "\n" + exception.ToString() : coloredLine;

            switch (level)
            {
                case LogLevel.Warn:
                    Verse.Log.Warning(text);
                    break;
                case LogLevel.Error:
                case LogLevel.Fatal:
                    Verse.Log.Error(text);
                    break;
                default:
                    Verse.Log.Message(text);
                    break;
            }
        }
    }
}
