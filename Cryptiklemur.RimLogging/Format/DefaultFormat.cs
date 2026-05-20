using System.Collections.Generic;

namespace Cryptiklemur.RimLogging.Format
{
    public static class DefaultFormat
    {
        public const string Default = "[{ts}] [{level}] [{channel}] [{source}] {message}{ctx}";

        public static string Render(string template, LogEntry entry, bool stripRichText)
        {
            return RenderInternal(template, entry, stripRichText, stopToken: null);
        }

        /// <summary>
        /// Renders all format tokens that appear <em>before</em> the <c>{message}</c> token,
        /// returning the prefix segment (including any literal text immediately preceding
        /// <c>{message}</c>). When the template contains no <c>{message}</c> token, the full
        /// rendered template is returned.
        /// </summary>
        /// <param name="template">The format template string.</param>
        /// <param name="entry">The log entry supplying token values.</param>
        /// <param name="stripRichText">When <c>true</c>, rich-text tags are stripped from token values.</param>
        public static string RenderPrefixOnly(string template, LogEntry entry, bool stripRichText)
        {
            return RenderInternal(template, entry, stripRichText, stopToken: "message");
        }

        private static string RenderInternal(string template, LogEntry entry, bool stripRichText, string? stopToken)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(template.Length + entry.RenderedMessage.Length + 64);
            int i = 0;
            while (i < template.Length)
            {
                char c = template[i];
                if (c == '{')
                {
                    int close = template.IndexOf('}', i + 1);
                    if (close < 0) { sb.Append(template, i, template.Length - i); break; }
                    string token = template.Substring(i + 1, close - i - 1);
                    if (stopToken != null && token == stopToken) return sb.ToString();
                    sb.Append(ResolveToken(token, entry, stripRichText));
                    i = close + 1;
                    continue;
                }
                sb.Append(c);
                i++;
            }
            return sb.ToString();
        }

        private static string ResolveToken(string token, LogEntry e, bool strip)
        {
            switch (token)
            {
                case "ts":      return e.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                case "level":   return e.Level.ToString().ToUpperInvariant();
                case "channel": return e.Channel;
                case "source":  return e.Source.IsCallerProvided ? e.Source.File + ":" + e.Source.Line : "?:0";
                case "message": return strip ? RichText.Strip(e.RenderedMessage) : e.RenderedMessage;
                case "ctx":     return RenderUnconsumedContext(e);
                default:        return "{" + token + "}";
            }
        }

        private static string RenderUnconsumedContext(LogEntry e)
        {
            if (e.Context == null || e.Context.Count == 0) return string.Empty;
            MessageTemplate t = TemplateCache.Get(e.MessageTemplate);
            bool any = false;
            System.Text.StringBuilder sb = new System.Text.StringBuilder(" {");
            foreach (KeyValuePair<string, object?> kv in e.Context)
            {
                if (t.Holes.Contains(kv.Key)) continue;
                if (any) sb.Append(", ");
                sb.Append(kv.Key).Append('=').Append(kv.Value);
                any = true;
            }
            sb.Append('}');
            return any ? sb.ToString() : string.Empty;
        }
    }
}
