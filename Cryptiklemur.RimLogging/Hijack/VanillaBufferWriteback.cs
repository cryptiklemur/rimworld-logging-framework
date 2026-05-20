namespace Cryptiklemur.RimLogging.Hijack
{
    /// <summary>
    /// Routes a fully-rendered colored line into vanilla's <c>Verse.Log</c> buffer via reflection-confirmed
    /// method dispatch, wrapped in a <see cref="Pipeline.ReentryGuard"/> scope so Harmony prefix patches
    /// (Task 5.4) can detect and short-circuit recursive calls. After each vanilla write,
    /// <c>Verse.Log.ResetMessageCount</c> is invoked via reflection to prevent duplicate-suppression from
    /// silencing fresh entries. The <c>_messagesField</c> reference is resolved at startup for use by the
    /// Phase 9 bug-report bundle; it is not consumed here.
    /// </summary>
    internal static class VanillaBufferWriteback
    {
        /// <summary>Backing field for the in-game console message list. Reserved for Phase 9 bug-report bundle.</summary>
        private static readonly System.Reflection.FieldInfo? _messagesField;

        /// <summary>Prevents duplicate-suppression by resetting the vanilla message counter after each write.</summary>
        private static readonly System.Reflection.MethodInfo? _resetMessageCount;

        static VanillaBufferWriteback()
        {
            System.Type logT = typeof(Verse.Log);
            _messagesField = logT.GetField(
                "messages",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            _resetMessageCount = logT.GetMethod(
                "ResetMessageCount",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        }

        /// <summary>
        /// Writes <paramref name="coloredLine"/> into the appropriate vanilla log method based on
        /// <paramref name="level"/>, guarded by <see cref="Pipeline.ReentryGuard"/> to prevent recursion.
        /// </summary>
        /// <param name="level">Severity level — determines which <c>Verse.Log</c> method is called.</param>
        /// <param name="coloredLine">Fully-rendered, color-tagged line to write.</param>
        /// <param name="exception">
        /// Informational only. The default format template (added in Phase 4 Task 4.9) already embeds
        /// the exception text via the <c>{exc}</c> token, so <paramref name="coloredLine"/> already
        /// contains exception details. This parameter is retained for API compatibility with
        /// <c>VerseLogSink</c> callers.
        /// </param>
        public static void Write(LogLevel level, string coloredLine, System.Exception? exception)
        {
            _ = exception;

            using (Pipeline.ReentryGuard.Enter())
            {
                switch (level)
                {
                    case LogLevel.Trace:
                    case LogLevel.Debug:
                    case LogLevel.Info:
                        Verse.Log.Message(coloredLine);
                        break;
                    case LogLevel.Warn:
                        Verse.Log.Warning(coloredLine);
                        break;
                    case LogLevel.Error:
                    case LogLevel.Fatal:
                        Verse.Log.Error(coloredLine);
                        break;
                }

                _resetMessageCount?.Invoke(null, null);
            }
        }
    }
}
