using System.Collections.Generic;
using UnityEngine;

namespace Cryptiklemur.RimLogging.Channels;

public class ChannelDef : Verse.Def
{
    public LogLevel defaultLevel = LogLevel.Info;
    public Color? color = null;
    public LogLevel captureStackAt = LogLevel.Error;
    public List<string> destinations = new List<string>();
    public string? format = null;

    public string? ColorHex => color.HasValue
        ? ColorUtility.ToHtmlStringRGB(color.Value)
        : null;
}
