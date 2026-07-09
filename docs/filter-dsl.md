# Filter DSL

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
