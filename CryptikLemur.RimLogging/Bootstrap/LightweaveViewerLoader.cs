using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace CryptikLemur.RimLogging.Bootstrap;

/// <summary>
/// Late-loads the optional Lightweave log viewer companion assembly when <c>Cosmere.Lightweave</c> is
/// active. The viewer DLL ships outside any <c>Assemblies/</c> folder so RimWorld never auto-validates
/// it at mod-load time (its base types live in Lightweave, which loads after RimLogging). Loading it
/// here, at <see cref="StaticConstructorOnStartup"/>, runs after every mod's assemblies are present, so
/// all Lightweave references resolve. This type holds no compile-time reference to the viewer or to
/// Lightweave; it drives them purely through reflection and Harmony.
/// </summary>
[StaticConstructorOnStartup]
internal static class LightweaveViewerLoader
{
    private const string LightweavePackageId = "cosmere.lightweave";
    private const string SelfPackageId = "cryptiklemur.rimlogging";
    private const string ViewerFolder = "LightweaveViewer";
    private const string ViewerDll = "CryptikLemur.RimLogging.LightweaveViewer.dll";
    private const string BootTypeName = "CryptikLemur.RimLogging.LightweaveViewer.LogViewerBoot";

    static LightweaveViewerLoader()
    {
        try
        {
            if (ModLister.GetActiveModWithIdentifier(LightweavePackageId, ignorePostfix: true) == null)
            {
                return;
            }

            string? dllPath = ResolveViewerDllPath();
            if (dllPath == null || !File.Exists(dllPath))
            {
                Log.Warn("Lightweave is active but the log viewer assembly was not found at " + (dllPath ?? "<unknown>"));
                return;
            }

            Assembly viewer = Assembly.LoadFrom(dllPath);
            viewer.GetType(BootTypeName)
                ?.GetMethod("Init", BindingFlags.Public | BindingFlags.Static)
                ?.Invoke(null, null);
            new Harmony("cryptiklemur.rimlogging.lightweaveviewer").PatchAll(viewer);
        }
        catch (Exception ex)
        {
            Log.Warn("Failed to load the Lightweave log viewer: " + ex);
        }
    }

    private static string? ResolveViewerDllPath()
    {
        foreach (ModContentPack pack in LoadedModManager.RunningModsListForReading)
        {
            if (pack.PackageId.Equals(SelfPackageId, StringComparison.OrdinalIgnoreCase))
            {
                return Path.Combine(pack.RootDir, ViewerFolder, ViewerDll);
            }
        }
        return null;
    }
}
