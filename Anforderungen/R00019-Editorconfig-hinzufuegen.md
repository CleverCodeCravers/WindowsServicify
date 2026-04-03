---
id: R00019
titel: ".editorconfig hinzufuegen"
typ: Verbesserung
status: Offen
prioritaet: Niedrig
aufwand: Klein
erstellt: 2026-04-03
quelle: R00001-Wartung-2026-04-03 (Vorschlag 11)
---

# R00019: .editorconfig hinzufuegen

## Beschreibung

Keine `.editorconfig` vorhanden — keine einheitliche Formatierung oder Coding-Style-Vorgaben. Unterschiedliche IDEs und Editoren koennen unterschiedliche Formatierungen erzeugen.

## Akzeptanzkriterien

- [ ] `.editorconfig` im Solution-Verzeichnis erstellt
- [ ] Standard-.NET-Konventionen konfiguriert (Einrueckung, Namenskonventionen, etc.)
- [ ] Optional: Roslyn-Analyzer (`Microsoft.CodeAnalysis.NetAnalyzers`) hinzugefuegt

## Umsetzung

1. `dotnet new editorconfig` im Solution-Verzeichnis ausfuehren
2. Ggf. an Projekt-Konventionen anpassen
3. Optional: `Microsoft.CodeAnalysis.NetAnalyzers` als NuGet-Paket hinzufuegen
