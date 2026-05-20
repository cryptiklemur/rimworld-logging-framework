using System;
using System.Collections.Generic;

namespace Cryptiklemur.RimLogging.Format;

/// <summary>Parsed structured-logging template with holes and literal segments.</summary>
public sealed class MessageTemplate
{
    /// <summary>Original string with {Name}-style holes.</summary>
    public string Raw { get; }

    /// <summary>Named placeholders in order of appearance.</summary>
    public IReadOnlyList<string> Holes { get; }

    /// <summary>Literal text between holes (Segments.Count == Holes.Count + 1 always).</summary>
    public IReadOnlyList<string> Segments { get; }

    public MessageTemplate(string raw, IReadOnlyList<string> holes, IReadOnlyList<string> segments)
    {
        Raw = raw ?? throw new ArgumentNullException(nameof(raw));
        Holes = holes ?? throw new ArgumentNullException(nameof(holes));
        Segments = segments ?? throw new ArgumentNullException(nameof(segments));
    }

    /// <summary>Parses a raw template string into named holes and surrounding literal segments.</summary>
    public static MessageTemplate Parse(string raw)
    {
        if (raw == null) throw new ArgumentNullException(nameof(raw));
        if (raw.Length == 0) return new MessageTemplate(raw, Array.Empty<string>(), new[] { string.Empty });

        System.Text.StringBuilder seg = new System.Text.StringBuilder();
        List<string> holes = new List<string>();
        List<string> segments = new List<string>();
        int i = 0;
        while (i < raw.Length)
        {
            char c = raw[i];
            if (c == '{' && i + 1 < raw.Length && raw[i + 1] == '{')
            {
                seg.Append('{');
                i += 2;
                continue;
            }
            if (c == '}' && i + 1 < raw.Length && raw[i + 1] == '}')
            {
                seg.Append('}');
                i += 2;
                continue;
            }
            if (c == '{')
            {
                int close = raw.IndexOf('}', i + 1);
                if (close < 0)
                {
                    seg.Append(raw, i, raw.Length - i);
                    i = raw.Length;
                    continue;
                }
                string name = raw.Substring(i + 1, close - i - 1);
                if (name.Length == 0 || ContainsBrace(name))
                {
                    seg.Append(raw, i, close - i + 1);
                    i = close + 1;
                    continue;
                }
                segments.Add(seg.ToString());
                seg.Clear();
                holes.Add(name);
                i = close + 1;
                continue;
            }
            seg.Append(c);
            i++;
        }
        segments.Add(seg.ToString());
        return new MessageTemplate(raw, holes, segments);
    }

    /// <summary>Renders the template by substituting positional args into holes and returns the formatted string plus a name-to-value context dictionary.</summary>
    public (string Rendered, IReadOnlyDictionary<string, object?>? Context) Render(object?[] args)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder(Raw.Length + 16);
        Dictionary<string, object?>? ctx = Holes.Count > 0 ? new Dictionary<string, object?>(Holes.Count) : null;
        for (int i = 0; i < Holes.Count; i++)
        {
            sb.Append(Segments[i]);
            if (i < args.Length)
            {
                object? v = args[i];
                sb.Append(FormatArg(v));
                ctx![Holes[i]] = v;
            }
            else
            {
                sb.Append('{').Append(Holes[i]).Append('}');
            }
        }
        sb.Append(Segments[Holes.Count]);
        return (sb.ToString(), ctx);
    }

    private static string FormatArg(object? v) => v switch
    {
        null => string.Empty,
        string s => s,
        IFormattable f => f.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
        _ => v.ToString() ?? string.Empty,
    };

    private static bool ContainsBrace(string s)
    {
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == '{' || s[i] == '}') return true;
        }
        return false;
    }
}
