---
id: R00013
titel: "ProcessLogger-Performance optimieren"
typ: Verbesserung
status: Offen
prioritaet: Mittel
aufwand: Klein
erstellt: 2026-04-03
quelle: R00001-Wartung-2026-04-03 (Vorschlag 5)
---

# R00013: ProcessLogger-Performance optimieren

## Beschreibung

`ProcessLogger.Log()` fuehrt bei jedem einzelnen Log-Aufruf `EnsureLogFilePathExists()` und `RemoveOldLogs()` aus. Bei haeufigem Logging entsteht erheblicher I/O-Overhead.

## Ist-Zustand

- `ProcessLogger.cs:13-21`: Jeder `Log()`-Aufruf prueft Verzeichnis-Existenz und bereinigt alte Logs
- `RemoveOldLogs()` listet alle .log-Dateien auf und prueft Erstellungsdatum — bei jedem Log-Eintrag
- `File.AppendAllText()` oeffnet und schliesst die Datei bei jedem Aufruf

## Akzeptanzkriterien

- [ ] Verzeichnis-Existenz wird einmal im Konstruktor geprueft
- [ ] Log-Bereinigung erfolgt maximal einmal pro Tag (nicht bei jedem Log-Aufruf)
- [ ] File-I/O ist optimiert (z.B. StreamWriter mit Buffer statt AppendAllText)
- [ ] Logging-Verhalten bleibt identisch (gleiches Format, gleiche Rotation)

## Umsetzung

1. `EnsureLogFilePathExists()` in den Konstruktor verschieben
2. `_lastCleanup`-Feld einfuehren, `RemoveOldLogs()` nur ausfuehren wenn letzter Cleanup > 24h her
3. Optional: `StreamWriter` mit AutoFlush statt `File.AppendAllText`
