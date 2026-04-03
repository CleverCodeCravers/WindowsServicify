---
id: R00014
titel: "StdErr erfassen und Event-Reihenfolge korrigieren"
typ: Bugfix
status: Offen
prioritaet: Mittel
aufwand: Klein
erstellt: 2026-04-03
quelle: R00001-Wartung-2026-04-03 (Vorschlag 6)
---

# R00014: StdErr erfassen und Event-Reihenfolge korrigieren

## Beschreibung

Der ProcessManager erfasst Standard Error nicht und registriert den OutputDataReceived-Handler nach dem Start des asynchronen Lesens — fruehe Ausgaben koennen verloren gehen.

## Ist-Zustand

- `ProcessManager.cs:50`: `BeginOutputReadLine()` wird VOR Event-Handler-Registrierung (Zeile 52) aufgerufen
- `ProcessManager.cs:43-59`: `RedirectStandardError` ist nicht gesetzt — Fehlermeldungen des ueberwachten Prozesses sind unsichtbar

## Akzeptanzkriterien

- [ ] Event-Handler werden vor `BeginOutputReadLine()` / `BeginErrorReadLine()` registriert
- [ ] StdErr wird erfasst und ueber ProcessLogger geloggt
- [ ] StdErr-Eintraege sind im Log als solche erkennbar (z.B. Prefix `[ERROR]`)
- [ ] Keine Ausgaben gehen beim Prozessstart verloren

## Umsetzung

```csharp
_process.StartInfo.RedirectStandardError = true;
_process.OutputDataReceived += (sender, args) => { /* log */ };
_process.ErrorDataReceived += (sender, args) => { /* log with [ERROR] prefix */ };
_process.Start();
_process.BeginOutputReadLine();
_process.BeginErrorReadLine();
```
