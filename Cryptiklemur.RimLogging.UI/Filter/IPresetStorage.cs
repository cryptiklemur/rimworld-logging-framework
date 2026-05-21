namespace Cryptiklemur.RimLogging.UI.Filter;

// Production storage adapter (LoggingMod.Settings + Write) lives in UI bootstrap -- see Phase 10.
internal interface IPresetStorage
{
    System.Collections.Generic.List<string> Names { get; }
    System.Collections.Generic.List<string> Expressions { get; }
    void Persist();
}
