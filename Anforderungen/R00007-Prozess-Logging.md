---
id: R00007
titel: "Prozess-Logging"
typ: Feature
status: Implementiert
erstellt: 2026-04-03
---

# R00007: Prozess-Logging

## Beschreibung

Als Administrator moechte ich, dass alle Ausgaben des ueberwachten Prozesses in Log-Dateien geschrieben werden, damit ich Probleme nachvollziehen kann.

## Akzeptanzkriterien

- [x] Log-Dateien werden im Verzeichnis der EXE erstellt
- [x] Dateiname-Format: `yyyy-MM-dd.log` (eine Datei pro Tag)
- [x] Jeder Log-Eintrag hat einen Zeitstempel: `[yyyy-MM-dd HH:mm:ss] Nachricht`
- [x] Stdout des ueberwachten Prozesses wird in die Log-Datei geschrieben
- [x] Log-Verzeichnis wird automatisch erstellt falls nicht vorhanden
- [x] Log-Dateien aelter als 7 Tage werden automatisch geloescht

## Implementierung

- `ProcessLogger.cs` — Logging mit Tages-Rotation und automatischer Bereinigung
- `ProcessManager.cs:52-57` — OutputDataReceived-Event leitet an ProcessLogger weiter

## Bekannte Probleme

- StdErr wird nicht erfasst (siehe R00015)
- RemoveOldLogs() wird bei jedem Log-Aufruf ausgefuehrt (siehe R00014)
