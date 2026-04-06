---
id: R00028
titel: "Exit-Codes in Program.cs einfuehren"
typ: Verbesserung
status: Abgeschlossen
prioritaet: Mittel
aufwand: Klein
erstellt: 2026-04-06
quelle: R00022-Wartung-2026-04-06 (Vorschlag 8)
---

# R00028: Exit-Codes in Program.cs einfuehren

## Beschreibung

`Program.cs` nutzt Top-Level Statements ohne Rueckgabewert. Fehlerhafte Ausfuehrung (fehlende config.json, ungueltige Argumente) beendet mit Exit-Code 0, was Script-Integration und CI-Nutzung erschwert.

## Ist-Zustand

- `Program.cs` gibt keinen Exit-Code zurueck
- `return;` ohne Wert an mehreren Stellen (Zeile 20, 37, 46, 80)
- Fehlerszenarien enden mit Exit-Code 0 (wie Erfolg)
- Architektur-Standard fordert: `Main()` gibt `int` zurueck — 0 fuer Erfolg, non-zero fuer Fehler

## Akzeptanzkriterien

- [ ] `return 0` bei erfolgreichem Abschluss
- [ ] `return 1` bei Parse-Fehlern (ungueltige Argumente)
- [ ] `return 1` bei fehlender oder ungueltiger config.json
- [ ] `return 1` bei Installations-/Deinstallationsfehlern
- [ ] Alle bestehenden Tests gruen

## Umsetzung

1. Top-Level Statements um `return 0` / `return 1` ergaenzen
2. Fehlerszenarien identifizieren und mit `return 1` beenden:
   - Zeile 17-20: Parse-Fehler → `return 1`
   - Zeile 50: Load-Fehler im Testrun → `return 1`
   - Zeile 83: Load-Fehler im Service-Modus → Bereits `Environment.Exit(1)` in WindowsBackgroundService
   - Zeile 119, 127: Load-Fehler bei Install/Uninstall → `return 1`

## Betroffene Dateien

| Datei | Aenderung |
|-------|-----------|
| `Source/WindowsServicify/WindowsServicify.ConsoleApp/Program.cs` | Return-Statements hinzufuegen |

## Abhaengigkeiten

- Abhaengig von: R00027 (Result-Pattern — wenn Load() Result zurueckgibt, sind Exit-Codes einfacher zu implementieren)
