namespace Cryptiklemur.RimLogging.Filters
{
    /// <summary>
    /// Describes a named log channel and its optional display color.
    /// Phase 6 will expand this with <c>Verse.Def</c> inheritance and the full filter DSL;
    /// for now it carries only the color override consumed by <c>VerseLogSink</c>.
    /// </summary>
    public class ChannelDef
    {
        /// <summary>
        /// Optional hex color string (no <c>#</c> prefix) used by <c>VerseLogSink</c> to
        /// wrap the log prefix in a <c>&lt;color=#…&gt;</c> tag. When <c>null</c>, the
        /// sink falls back to the severity color from <see cref="Cryptiklemur.RimLogging.Format.SeverityColors"/>.
        /// </summary>
        public string? ColorHex;
    }
}
