# RimLogging

A public, structured logging framework for RimWorld 1.6+ mods.

> Status: pre-v0.1. APIs are not stable yet. Do not depend on this from a shipped mod.

## What it is

Replaces vanilla `Verse.Log` and `UnityEngine.Debug.Log` with a single structured pipeline:

- Hierarchical channels (XML defs or transient) with prefix-based filtering.
- Serilog-style templated messages: `Log.Info("player {Name} died at {Hp}hp", "Bob", 5)`.
- Anonymous-object structured context: `Log.Info("died", new { pawn, hp })`.
- Six levels: `Trace`, `Debug`, `Info`, `Warn`, `Error`, `Fatal`.
- Multi-sink output: Verse log writeback, rolling text file, NDJSON file, in-memory (tests), plus a plugin sink API.
- Expression-based filter DSL for the in-game viewer (`level >= Warn OR channel = "Cosmere.*"`).
- Three-pane in-game log viewer (when Lightweave is installed).
- Lock-free MPSC queue + background drain. Synchronous bypass for `Error` / `Fatal`.

## Modules

| Assembly | Depends on | Purpose |
|---|---|---|
| `Cryptiklemur.RimLogging` | Harmony, RimWorld 1.6 | Core pipeline, sinks, channels, DSL parser, Verse.Log + Unity hijack. |
| `Cryptiklemur.RimLogging.UI` | + Lightweave | Three-pane log viewer that replaces the vanilla debug log window. Soft-fails if Lightweave is missing. |

## Install

### As a Steam Workshop dependency

Add `cryptiklemur.rimlogging` to your `About.xml`'s `<modDependencies>`. End-users install the Workshop mod once and every consumer mod shares a single dll.

### As a NuGet package (bundled)

```
dotnet add package Cryptiklemur.RimLogging
```

The dll is copied into your mod's `Assemblies/` directory at build time. No Workshop dependency required.

> Trade-off: if multiple mods each NuGet-bundle their own copy, each loads its own assembly. Static state (channel registry, sink list, MPSC queue) is per-assembly. Prefer the Workshop dep when multiple consumers are expected.

## Usage

```csharp
using Cryptiklemur.RimLogging;

Log.Info("colony {Name} founded at {Tile}", colony.Name, colony.Tile);
Log.Warn("low food", new { pawn, days_left = 2 });
Log.Error(ex, "save failed");
Log.Error("save failed", new { ex, path });

Log.Info("Cosmere.Roshar.Surgebinding", "bond formed with {Spren}", spren.Label);
```

## ChannelDef XML

```xml
<RimLogging.ChannelDef>
    <defName>Cosmere.Roshar.Surgebinding</defName>
    <label>Surgebinding</label>
    <description>Stormlight bonding, surge investiture, oath progression.</description>
    <defaultLevel>Debug</defaultLevel>
    <color>(0.7, 0.85, 1.0)</color>
    <captureStackAt>Error</captureStackAt>
</RimLogging.ChannelDef>
```

## Filter DSL

```
level >= Warn
level >= Warn OR channel = "Cosmere.*"
channel = "Cosmere.Roshar.*" AND level >= Debug
NOT (channel = "Unity")
```

## Building

```
make build      # whole solution
make test       # xunit suites
make format     # dotnet format
```

## License

MIT.
