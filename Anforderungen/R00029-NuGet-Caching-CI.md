---
id: R00029
titel: "NuGet-Caching im CI-Workflow"
typ: Verbesserung
status: Abgeschlossen
prioritaet: Mittel
aufwand: Klein
erstellt: 2026-04-06
quelle: R00022-Wartung-2026-04-06 (Vorschlag 9)
---

# R00029: NuGet-Caching im CI-Workflow

## Beschreibung

Jeder CI-Lauf laedt alle NuGet-Pakete neu herunter. Durch Caching koennen Build-Zeiten reduziert werden.

## Ist-Zustand

- `build.yml`: `dotnet restore` ohne Cache
- Jeder Build-Lauf laedt ~5 NuGet-Pakete herunter

## Akzeptanzkriterien

- [ ] NuGet-Packages werden im CI gecacht
- [ ] Cache-Key basiert auf .csproj-Dateien (invalidiert bei Abhaengigkeitsaenderungen)
- [ ] Restore-Zeiten bei Cache-Hit deutlich reduziert
- [ ] Build und Tests weiterhin gruen

## Umsetzung

In `build.yml` vor dem Restore-Step einfuegen:
```yaml
- uses: actions/cache@v4
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
    restore-keys: ${{ runner.os }}-nuget-
```

## Betroffene Dateien

| Datei | Aenderung |
|-------|-----------|
| `.github/workflows/build.yml` | Cache-Step hinzufuegen |
