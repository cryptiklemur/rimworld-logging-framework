namespace Cryptiklemur.RimLogging.Pipeline;

internal static class SynchronousBypass
{
    public static bool ShouldBypass(LogLevel level) => level >= LogLevel.Error;
}
