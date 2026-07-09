# Contributing

## Building

```
make build      # whole solution
make test       # xunit suites
make format     # dotnet format
```

Run `make build` and `make test` green before opening a PR.

## Commits and versioning

Versions come from [Conventional Commits](https://www.conventionalcommits.org/) via semantic-release, so commit messages double as release notes:

- `fix:` / `perf:` -> patch
- `feat:` -> minor
- `BREAKING CHANGE:` / `!` -> major

Releases are git tags. Stable releases are cut from `main`; the `beta` branch publishes prereleases.

## Translations

English is the source language. The other bundled translations (Chinese Simplified, French, Spanish, German) may be inaccurate. Corrections are welcome via pull request.
