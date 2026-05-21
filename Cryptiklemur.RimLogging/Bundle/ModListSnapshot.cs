namespace Cryptiklemur.RimLogging.Bundle;

public static class ModListSnapshot
{
    public static System.Collections.Generic.List<BundlePayload.ModInfo> Capture()
    {
        System.Collections.Generic.List<BundlePayload.ModInfo> result = new System.Collections.Generic.List<BundlePayload.ModInfo>();
        foreach (Verse.ModContentPack mcp in Verse.LoadedModManager.RunningMods)
        {
            result.Add(new BundlePayload.ModInfo
            {
                Name = mcp.Name ?? "",
                PackageId = mcp.PackageId ?? "",
                Version = ManifestVersionReader.TryGetVersion(mcp.RootDir),
                Active = true,
            });
        }
        return result;
    }
}
