namespace Cryptiklemur.RimLogging.Bundle;

public static class ManifestVersionReader
{
    public static string? TryGetVersion(string rootDir)
    {
        try
        {
            string path = System.IO.Path.Combine(rootDir, "About", "Manifest.xml");
            if (!System.IO.File.Exists(path)) return null;
            System.Xml.Linq.XDocument doc = System.Xml.Linq.XDocument.Load(path);
            System.Xml.Linq.XElement? v = doc.Root?.Element("version");
            return v?.Value;
        }
        catch
        {
            return null;
        }
    }
}
