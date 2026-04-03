---
id: R00018
titel: "Graceful Shutdown implementieren"
typ: Verbesserung
status: Offen
prioritaet: Niedrig
aufwand: Klein
erstellt: 2026-04-03
quelle: R00001-Wartung-2026-04-03 (Vorschlag 10)
---

# R00018: Graceful Shutdown implementieren

## Beschreibung

`ProcessManager.Stop()` ruft sofort `_process.Kill()` auf. Der ueberwachte Prozess hat keine Moeglichkeit, sich sauber zu beenden (offene Dateien schliessen, Transaktionen abschliessen, etc.).

## Ist-Zustand

- `ProcessManager.cs:63-67`: `Stop()` ruft direkt `Kill()` auf
- Kein Versuch eines sanften Beendens

## Akzeptanzkriterien

- [ ] Bei Stop wird zuerst ein sanftes Beenden versucht (Ctrl+C-Signal oder CloseMainWindow)
- [ ] Konfigurierbarer Timeout (z.B. 10 Sekunden) bevor hart beendet wird
- [ ] Falls Prozess nach Timeout noch laeuft: `Kill(entireProcessTree: true)`
- [ ] Verhalten wird im Log dokumentiert ("Sending shutdown signal...", "Force-killing after timeout")

## Umsetzung

1. `GenerateConsoleCtrlEvent` (P/Invoke) oder `_process.CloseMainWindow()` als ersten Versuch
2. Timeout abwarten (konfigurierbar, Default 10s)
3. Falls Prozess noch laeuft: `_process.Kill(entireProcessTree: true)` (.NET 7+ API)
4. Logging fuer jeden Schritt
