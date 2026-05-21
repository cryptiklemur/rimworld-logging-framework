using System.Collections.Generic;

namespace Cryptiklemur.RimLogging.Bundle;

public sealed class BundlePayload
{
    public string RimWorldVersion { get; set; } = "";
    public string FrameworkVersion { get; set; } = "";
    public List<ModInfo> Mods { get; set; } = new List<ModInfo>();
    public List<EntryDto> Entries { get; set; } = new List<EntryDto>();

    public sealed class ModInfo
    {
        public string Name { get; set; } = "";
        public string PackageId { get; set; } = "";
        public string? Version { get; set; }
        public bool Active { get; set; }
    }

    public sealed class EntryDto
    {
        public string Ts { get; set; } = "";
        public string Level { get; set; } = "";
        public string Channel { get; set; } = "";
        public string Source { get; set; } = "";
        public string Msg { get; set; } = "";
        public Dictionary<string, object?>? Ctx { get; set; }
        public string? Stack { get; set; }
    }
}
