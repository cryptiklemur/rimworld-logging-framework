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
}
