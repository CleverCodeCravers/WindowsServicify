---
id: R00006
titel: "Windows Background Service mit Auto-Restart"
typ: Feature
status: Implementiert
erstellt: 2026-04-03
---

# R00006: Windows Background Service mit Auto-Restart

## Beschreibung

Als Administrator moechte ich, dass WindowsServicify als Windows-Dienst laeuft und den konfigurierten Prozess dauerhaft ueberwacht und bei Absturz automatisch neustartet.

## Akzeptanzkriterien

- [x] WindowsServicify registriert sich als Windows Background Service via `Microsoft.Extensions.Hosting.WindowsServices`
- [x] Der konfigurierte Prozess wird beim Service-Start gestartet
- [x] Alle 5 Sekunden wird geprueft, ob der Prozess noch laeuft
- [x] Bei Prozess-Absturz wird automatisch neugestartet (mit Log-Eintrag)
- [x] Bei Service-Stop wird der ueberwachte Prozess beendet
- [x] Bei kritischen Fehlern wird `Environment.Exit(1)` aufgerufen, damit Windows-Recovery-Optionen greifen

## Implementierung

- `WindowsBackgroundService.cs` — BackgroundService-Implementation mit Watchdog-Schleife
- `ProcessManager.cs` — Prozess-Steuerung (Start/Stop/IsCorrectlyRunning)
