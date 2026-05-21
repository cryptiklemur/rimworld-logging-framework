namespace Cryptiklemur.RimLogging.Filters
{
    /// <summary>
    /// Resolves channel names to their <see cref="ChannelDef"/> definitions.
    /// Phase 6 populates the registry from <c>Verse.Def</c> loading; this stub
    /// always returns <c>null</c> so callers fall back to severity colors.
    /// </summary>
    public static class ChannelRegistry
    {
        /// <summary>
        /// Returns all <see cref="ChannelDef"/> instances registered with the framework.
        /// Returns an empty list until Phase 10 populates the registry during Def loading.
        /// </summary>
        public static IReadOnlyList<ChannelDef> AllRegisteredDefs => [];

        /// <summary>
        /// Returns the <see cref="ChannelDef"/> registered for <paramref name="channel"/>,
        /// or <c>null</c> when no definition has been registered.
        /// Phase 6 will populate the registry during <c>Verse.Def</c> loading.
        /// </summary>
        /// <param name="channel">The dot-separated channel name to look up.</param>
        public static ChannelDef? TryResolve(string channel) => null;
    }
}
