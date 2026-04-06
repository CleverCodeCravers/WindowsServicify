---
id: R00026
titel: "Integration-Test-Projekt anlegen"
typ: Verbesserung
status: Abgeschlossen
prioritaet: Mittel
aufwand: Mittel
erstellt: 2026-04-06
quelle: R00022-Wartung-2026-04-06 (Vorschlag 6)
---

# R00026: Integration-Test-Projekt anlegen

## Beschreibung

5 Klassen (ConsoleInput, LegacyWindowsServiceInstallHelper, PowerShellWindowsServiceInstallHelper, ServiceConfigurationRequester, WindowsBackgroundService) sind per `coverlet.runsettings` von der Coverage ausgeschlossen, da sie Windows-Service-APIs und Console-I/O nutzen. Ein Integration-Test-Projekt wuerde diese Luecke schliessen.

## Ist-Zustand

- Nur Unit-Tests vorhanden (`WindowsServicify.Domain.Tests`)
- 71.7% Line Coverage (100% auf testbaren Klassen, 0% auf Infrastruktur)
- Kein Integration-Test-Projekt
- Globaler Architektur-Standard (`dotnet-cli-tool`) fordert `<Name>.BL.IntegrationTests/`

## Akzeptanzkriterien

- [x] Neues Projekt `WindowsServicify.Domain.IntegrationTests` in Solution
- [x] NUnit 4.x als Test-Framework (konsistent mit Unit-Tests)
- [x] Mindestens ein Test fuer den configure/testrun-Flow (E2E-artig: config schreiben → Prozess starten → Output pruefen → stoppen)
- [x] Mindestens ein Test fuer WindowsBackgroundService Lifecycle (Start → Running → Stop)
- [x] Tests nutzen temporaere Verzeichnisse mit Cleanup
- [x] Coverage-Messung ueber Solution (Unit + Integration gemeinsam)
- [x] CI-Workflow fuehrt auch Integration-Tests aus

## Umsetzung

1. Projekt `WindowsServicify.Domain.IntegrationTests` anlegen mit NUnit 4.x
2. Zur Solution hinzufuegen
3. Testinfrastruktur: Temporaere Verzeichnisse, Config-Dateien, Testprozesse
4. Tests implementieren:
   - Testrun-Flow: Config schreiben → ProcessManager starten → Output in Log pruefen
   - WindowsBackgroundService: Host starten → Prozess laeuft → Host stoppen → Prozess beendet
5. `coverlet.runsettings` ggf. anpassen (Ausschluesse fuer Integration-Tests reduzieren)
6. CI-Workflow anpassen: `dotnet test` ueber Solution deckt automatisch beide Projekte ab

## Betroffene Dateien

| Datei | Aenderung |
|-------|-----------|
| `Source/WindowsServicify/WindowsServicify.Domain.IntegrationTests/` | Neues Projekt |
| `Source/WindowsServicify/WindowsServicify.sln` | Projekt hinzufuegen |
| `.github/workflows/build.yml` | Ggf. anpassen |
