using Cryptiklemur.RimLogging.Filters;
using Cryptiklemur.RimLogging.Format;
using Cryptiklemur.RimLogging.Hijack;

namespace Cryptiklemur.RimLogging.Sinks
{
    /// <summary>
    /// Writes log entries back into vanilla's <c>Verse.Log</c> buffer, wrapping the
    /// prefix in a <c>&lt;color=#…&gt;</c> rich-text tag sourced from the channel's
    /// <see cref="ChannelDef.ColorHex"/> or, when absent, from
    /// <see cref="SeverityColors.GetHex"/>.
    /// </summary>
    /// <remarks>
    /// Integration coverage (color wrapping, ChannelDef override, writeback routing)
    /// comes in Phase 6 once <see cref="ChannelRegistry"/> is populated and
    /// <see cref="VanillaBufferWriteback"/> is exercised end-to-end.
    /// This file lives in <c>Sinks/</c> and is excluded from test projects that
    /// cannot reference Verse.
    /// </remarks>
    public sealed class VerseLogSink : ILogSink
    {
        /// <inheritdoc/>
        public string Name => "VerseLog";

        /// <summary>Gets or sets the minimum level; entries below this level are dropped.</summary>
        public LogLevel MinLevel { get; set; }

        /// <summary>Gets or sets the format template used to render the log prefix.</summary>
        public string FormatTemplate { get; set; } = DefaultFormat.Default;

        /// <summary>
        /// Initializes a new <see cref="VerseLogSink"/>.
        /// </summary>
        /// <param name="minLevel">Entries below this level are silently dropped. Defaults to <see cref="LogLevel.Trace"/>.</param>
        public VerseLogSink(LogLevel minLevel = LogLevel.Trace) { MinLevel = minLevel; }

        /// <inheritdoc/>
        public void Write(LogEntry entry)
        {
            if (entry.Level < MinLevel) return;

            // Render the format template piece-by-piece to apply color ONLY to the
            // prefix segments (everything before {message}). The message body keeps
            // any inline rich-text the caller supplied.
            ChannelDef? def = ChannelRegistry.TryResolve(entry.Channel);
            string colorHex = def?.ColorHex ?? SeverityColors.GetHex(entry.Level);
            string prefix = DefaultFormat.RenderPrefixOnly(FormatTemplate, entry, stripRichText: false);
            string colored = "<color=#" + colorHex + ">" + prefix + "</color> " + entry.RenderedMessage;

            // Route into vanilla. Different vanilla method per level so the in-game
            // console color matches RimWorld's expectations.
            VanillaBufferWriteback.Write(entry.Level, colored, entry.Exception);
        }

        /// <inheritdoc/>
        public void Flush() { }

        /// <inheritdoc/>
        public void Dispose() { }
    }
}
