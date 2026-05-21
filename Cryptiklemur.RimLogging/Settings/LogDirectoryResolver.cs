using System.IO;

namespace Cryptiklemur.RimLogging.Settings;

public static class LogDirectoryResolver
{
    public static string EnsureUnderBase(string baseDir)
    {
        string dir = Path.Combine(baseDir, "RimLogging");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        return dir;
    }
}
