---
id: R00005
titel: "Testlauf-Modus (--testrun)"
typ: Feature
status: Implementiert
erstellt: 2026-04-03
---

# R00005: Testlauf-Modus (--testrun)

## Beschreibung

Als Benutzer moechte ich die konfigurierte Anwendung im Konsolenmodus testen koennen, bevor ich sie als Service installiere.

## Akzeptanzkriterien

- [x] `WindowsServicify.exe --testrun` startet den konfigurierten Prozess im Vordergrund
- [x] Die Konsole zeigt den Fortschritt mit Punkten an
- [x] Bei Prozess-Absturz wird automatisch neugestartet mit Meldung "Restarting process..."
- [x] Tastendruck beendet den Testlauf
- [x] Beim Beenden wird der ueberwachte Prozess gestoppt

## Implementierung

- `Program.cs:61-78` — Testrun-Logik mit Console.KeyAvailable-Schleife
- `ProcessManager` — Prozessverwaltung (Start/Stop/IsCorrectlyRunning)
