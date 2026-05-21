using System.Collections.Generic;

namespace Cryptiklemur.RimLogging.Bundle;

public static class BundlerSessionFactory
{
    public static BundlePayload BuildForRunningSession(IReadOnlyList<LogEntry> entries)
    {
        return Bundler.Build(
            entries,
            RimWorld.VersionControl.CurrentVersionString,
            BuildInfo.Revision,
            ModListSnapshot.Capture()
        );
    }
}
