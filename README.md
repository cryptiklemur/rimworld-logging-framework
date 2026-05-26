# RimLogging

A public, structured logging framework for RimWorld 1.6+ mods.

> Status: pre-1.0. APIs are still settling. The first public release will be `1.0.0-beta` cut from the `beta` branch.

## What it is

Replaces vanilla `Verse.Log` and `UnityEngine.Debug.Log` with a single structured pipeline:

- Hierarchical channels (XML defs or transient) with prefix-based resolution.
- Serilog-style templated messages: `Log.Info("player {Name} died at {Hp}hp", "Bob", 5)`.
- Anonymous-object structured context: `Log.Info("died", new { pawn, hp })`.
- Six levels: `Trace`, `Debug`, `Info`, `Warn`, `Error`, `Fatal`.
- Multi-sink output: Verse log writeback, rolling text file, rolling NDJSON file, in-memory (tests), plus a plugin sink API.
- Expression-based filter DSL for the in-game viewer (`level >= Warn OR channel = "Cosmere.*"`).
- Three-pane in-game log viewer (when Lightweave is installed).
- Lock-free MPSC queue + background drain. Synchronous bypass for `Error` / `Fatal`.

## Modules

| Assembly | Depends on | Purpose |
|---|---|---|
| `CryptikLemur.RimLogging` | Harmony, RimWorld 1.6 | Core pipeline, sinks, channels, DSL parser, Verse.Log + Unity hijack. |

The three-pane in-game log viewer lives in the separate [Lightweave](https://github.com/RimworldCosmere/Lightweave) mod, which consumes this framework's `Filtering` and `Bundle` public surface. It is not part of this repository.

## Install

### As a Steam Workshop dependency (recommended for shipped mods)

End-users install the Workshop mod once and every consumer mod shares a single dll. Declare it in your `About/About.xml`:

```xml
<modDependencies>
    <li>
        <packageId>CryptikLemur.RimLogging</packageId>
        <displayName>RimLogging</displayName>
        <steamWorkshopUrl>steam://url/CommunityFilePage/REPLACE_WITH_WORKSHOP_ID</steamWorkshopUrl>
    </li>
</modDependencies>
<loadAfter>
    <li>CryptikLemur.RimLogging</li>
</loadAfter>
```

### As a NuGet package (bundled)

```
dotnet add package CryptikLemur.RimLogging
```

The dll (and its `System.Text.Json` runtime dependencies) are copied into your mod's `Assemblies/` directory at build time. No Workshop dependency required.

## Multi-NuGet-bundle behavior

Static state (channel registry, sink list, MPSC queue, Harmony patches) is per-assembly. If multiple mods each NuGet-bundle their own copy of `CryptikLemur.RimLogging`, only the first-loaded copy wins the `Verse.Log` patch race; later copies detect that another instance already owns the patch and put themselves into **degraded mode**:

- `Logging.IsPrimary` returns `false` on the non-owning copies.
- A copy in degraded mode still serves its own callers but defers the global `Verse.Log` / Unity hijack to the primary.

If you expect several consumer mods, prefer the Workshop dependency so all of them share one assembly and one pipeline.

## Quick start

```csharp
using CryptikLemur.RimLogging;

// Default channel, templated message with positional args.
Log.Info("colony {Name} founded at {Tile}", colony.Name, colony.Tile);

// Structured context from an anonymous object.
Log.Warn("low food", new { pawn, days_left = 2 });

// Exceptions: pass the exception first, or fold it into the context.
Log.Error(ex, "save failed");
Log.Error("save failed", new { ex, path });

// Explicit channel + structured context.
Log.Info("Cosmere.Roshar.Surgebinding", "bond formed", new { spren = spren.Label });

// Explicit channel + templated args: pass the args as an explicit array so the
// channel overload is selected (a bare trailing value binds as structured context).
Log.Info("Cosmere.Roshar.Surgebinding", "bond formed with {Spren}", new object?[] { spren.Label });

// Lower-severity levels.
Log.Trace("tick {N}", ticks);
Log.Debug("pathfinding cache miss");
Log.Fatal("unrecoverable: {Reason}", reason);
```

The first string argument is the channel only when a later argument disambiguates the overload; `Log.Info("text", arg)` treats `"text"` as the message. Formatting is lazy: if no registered sink accepts the entry's level, the template is never rendered and the context object is never reflected.

## Channels

Channels are dotted, hierarchical names. Define them in XML to set defaults, or just pass any string at call time for a transient channel.

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
    <CryptikLemur.RimLogging.Channels.ChannelDef>
        <defName>Cosmere.Roshar.Surgebinding</defName>
        <label>Surgebinding</label>
        <description>Stormlight bonding, surge investiture, oath progression.</description>
        <defaultLevel>Debug</defaultLevel>
        <color>(0.7, 0.85, 1.0)</color>
        <captureStackAt>Error</captureStackAt>
        <destinations>
            <li>RollingText</li>
        </destinations>
        <format>[{Channel}] {Message}</format>
    </CryptikLemur.RimLogging.Channels.ChannelDef>
</Defs>
```

> The XML element name is the fully namespace-qualified type, `CryptikLemur.RimLogging.Channels.ChannelDef`, not `RimLogging.ChannelDef`.

`ChannelDef` fields:

| Field | Default | Meaning |
|---|---|---|
| `defaultLevel` | `Info` | Minimum level emitted on this channel. |
| `color` | none | RGB tuple for the viewer, e.g. `(0.7, 0.85, 1.0)`. |
| `captureStackAt` | `Error` | Level at/above which a stack trace is captured. |
| `destinations` | all sinks | Sink defNames this channel routes to (empty = every registered sink). |
| `format` | default | Per-channel format template override. |

**Transient fallback / prefix resolution:** when you log to a channel name with no exact `ChannelDef`, resolution walks up the dotted prefix to the nearest registered ancestor, then falls back to the built-in `default` channel. So `Cosmere.Roshar.Surgebinding.Windrunner` uses the `Cosmere.Roshar.Surgebinding` def if that is the closest registered ancestor.

Built-in channels: `default` (catch-all), `Vanilla` (captured `Verse.Log` calls), `Unity` (captured `UnityEngine.Debug.Log` calls).

## Filter DSL

Used by the in-game viewer to filter the live log. Grammar:

```
expr    := orExpr
orExpr  := andExpr ( "OR" andExpr )*
andExpr := notExpr ( "AND" notExpr )*
notExpr := "NOT" notExpr | "(" expr ")" | term
term    := "level" levelOp LEVEL | "channel" strOp STRING
levelOp := "=" | "!=" | "<" | "<=" | ">" | ">="
strOp   := "=" | "!="
LEVEL   := Trace | Debug | Info | Warn | Error | Fatal
STRING  := "double-quoted, supports * wildcards"
```

Channel string matching supports `*` wildcards; a trailing `.*` matches the channel itself or any dotted descendant.

Examples:

```
level >= Warn
level >= Warn OR channel = "Cosmere.*"
channel = "Cosmere.Roshar.*" AND level >= Debug
NOT (channel = "Unity")
level != Trace AND NOT channel = "Vanilla"
```

## Custom sinks

Implement `ILogSink` and register it, either from code or via a `SinkDef`.

```csharp
public sealed class MySink : ILogSink
{
    public string Name => "MySink";
    public LogLevel MinLevel => LogLevel.Info;

    public void Write(LogEntry entry) { /* render or store entry */ }
    public void Flush() { /* flush buffers */ }
    public void Dispose() { /* close handles */ }
}

// Register from a StaticConstructorOnStartup or your mod ctor:
Logging.RegisterSink(new MySink());
```

Or load it from XML so the bootstrap phase instantiates it:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
    <CryptikLemur.RimLogging.Sinks.SinkDef>
        <defName>MySink</defName>
        <label>My Sink</label>
        <sinkClass>MyMod.MySink, MyMod</sinkClass>
        <minLevel>Info</minLevel>
        <enabledByDefault>true</enabledByDefault>
    </CryptikLemur.RimLogging.Sinks.SinkDef>
</Defs>
```

`sinkClass` is an assembly-qualified type name. The implementation needs a public parameterless constructor for XML loading. Built-in sinks: `VerseLog`, `RollingText` (enabled by default), `RollingJson` (NDJSON, off by default).

## Settings

The in-game mod settings page exposes:

- **Global minimum level** (`globalMinLevel`) - drops every entry below this level before any sink sees it.
- **Log directory** (`logDirectory`) - where rolling files are written; normalized to a default under the game's persistent data path when left blank.
- **Retention count** (`retentionCount`) - number of rotated log files kept.
- **Bundle proxy URL** (`proxyUrl`) - upload endpoint for bug-report bundles.
- **Filter presets** - saved name/expression pairs for the viewer's filter DSL.

All settings persist across restarts via RimWorld's Scribe system.

## Bug bundle

The viewer's "share bundle" button serializes a JSON payload and uploads it through the configured proxy, then copies the returned URL to your clipboard (with a toast confirmation). The payload contains:

- RimWorld version and framework version.
- The active mod list (name, packageId, version, active flag).
- Recent log entries (timestamp, level, channel, source, message, structured context, stack trace).

Override the upload endpoint with the `proxyUrl` setting if you self-host the proxy.

## Versioning

Versions are derived automatically from [Conventional Commits](https://www.conventionalcommits.org/) via semantic-release:

- `fix:` / `perf:` -> patch
- `feat:` -> minor
- `BREAKING CHANGE:` / `!` -> major

Releases are git tags. The first public release is `1.0.0-beta` on the `beta` branch; once it stabilizes, `beta` fast-forwards to `main` for `1.0.0`.

## Building

```
make build      # whole solution
make test       # xunit suites
make format     # dotnet format
```

## Translations

English is the source language. The other bundled translations (Chinese Simplified, French, Spanish, German) may be inaccurate. Corrections are welcome via pull request.

## Contributing

- Run `make build` and `make test` green before opening a PR.

## License

MIT.
