---
id: R00012
titel: "Unit-Tests und Integration-Tests schreiben"
typ: Verbesserung
status: Offen
prioritaet: Hoch
aufwand: Mittel
erstellt: 2026-04-03
quelle: R00001-Wartung-2026-04-03 (Vorschlag 4)
---

# R00012: Unit-Tests und Integration-Tests schreiben

## Beschreibung

Das Test-Projekt `WindowsServicify.Domain.Tests` existiert, enthaelt aber keinen einzigen Test. Die Tests sind auch im CI-Workflow auskommentiert.

## Ist-Zustand

- `Class1.cs` enthaelt eine leere Klasse
- NUnit 3.13.3 als Test-Framework referenziert
- Kein Test-Schritt im CI-Workflow (build.yml:45-46 auskommentiert)
- Keine Code-Coverage-Messung

## Akzeptanzkriterien

- [ ] Leere `Class1.cs` geloescht
- [ ] NUnit auf Version 4.x aktualisiert
- [ ] Projekt-Referenz auf `WindowsServicify.Domain` hinzugefuegt
- [ ] Unit-Tests fuer:
  - [ ] `ConsoleCommandLineParser.Parse()` — verschiedene Argument-Kombinationen, ungueltige Eingaben
  - [ ] `ServiceConfigurationFileHandler.Load/Save()` — Round-Trip-Serialisierung
  - [ ] `ProcessLogger.RemoveOldLogs()` — Dateien aelter als 7 Tage werden geloescht, neuere nicht
  - [ ] `WindowsServiceInstallHelperFactory.Create()` — Legacy vs. PowerShell
  - [ ] Input-Validierung (nach R00009)
- [ ] Integration-Tests fuer:
  - [ ] `ProcessManager` — Start/Stop/IsCorrectlyRunning mit einem echten Test-Prozess
- [ ] Coverage-Tool (`coverlet.collector`) konfiguriert
- [ ] Test-Schritt im CI-Workflow aktiv
- [ ] Coverage-Schwelle: mindestens 70%

## Umsetzung

1. `Class1.cs` loeschen
2. NuGet-Pakete aktualisieren: NUnit 4.x, FluentAssertions, Moq, coverlet.collector
3. Projekt-Referenz auf Domain-Projekt hinzufuegen
4. Test-Klassen erstellen
5. CI-Workflow: Test-Schritt einkommentieren, Coverage-Report hinzufuegen
