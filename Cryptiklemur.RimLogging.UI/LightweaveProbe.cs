using System;

namespace Cryptiklemur.RimLogging.UI;

internal static class LightweaveProbe
{
    public static bool IsAvailable()
    {
        try
        {
            Type? t = Type.GetType("Cosmere.Lightweave.Runtime.LightweaveRoot, Lightweave", throwOnError: false);
            return t != null;
        }
        catch { return false; }
    }
}
