---
id: R00023
titel: "TreatWarningsAsErrors aktivieren"
typ: Verbesserung
status: Abgeschlossen
prioritaet: Hoch
aufwand: Klein
erstellt: 2026-04-06
quelle: R00022-Wartung-2026-04-06 (Vorschlag 2)
---

# R00023: TreatWarningsAsErrors aktivieren

## Beschreibung

Compiler-Warnungen koennen unbemerkt eingefuehrt werden, da keine der .csproj-Dateien `TreatWarningsAsErrors` aktiviert hat. Der globale Architektur-Standard (`dotnet-cli-tool`) fordert dies in allen Projekten.

## Ist-Zustand

- Keine .csproj-Datei enthaelt `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
- Aktuell 0 Warnungen — Aktivierung sollte sofort funktionieren
- Keine `Directory.Build.props` vorhanden

## Akzeptanzkriterien

- [ ] `TreatWarningsAsErrors` ist in allen Projekten aktiv
- [ ] Umsetzung via `Directory.Build.props` (zentral, nicht pro Projekt)
- [ ] Build ohne Fehler
- [ ] Alle 130+ Tests gruen

## Umsetzung

1. `Source/WindowsServicify/Directory.Build.props` erstellen:
   ```xml
   <Project>
     <PropertyGroup>
       <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
     </PropertyGroup>
   </Project>
   ```
2. `dotnet build` verifizieren
3. `dotnet test` verifizieren

## Betroffene Dateien

| Datei | Aenderung |
|-------|-----------|
| `Source/WindowsServicify/Directory.Build.props` | Neu erstellen |
