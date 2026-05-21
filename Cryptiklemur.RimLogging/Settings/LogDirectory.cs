using System.IO;
using UnityEngine;

namespace Cryptiklemur.RimLogging.Settings;

public static class LogDirectory
{
    public static string Default => LogDirectoryResolver.EnsureUnderBase(Application.persistentDataPath);
}
