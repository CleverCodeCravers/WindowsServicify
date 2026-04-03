---
id: R00008
titel: "Hilfe-Anzeige (--help)"
typ: Feature
status: Implementiert
erstellt: 2026-04-03
---

# R00008: Hilfe-Anzeige (--help)

## Beschreibung

Als Benutzer moechte ich eine Uebersicht aller verfuegbaren Kommandos sehen koennen.

## Akzeptanzkriterien

- [x] `WindowsServicify.exe --help` zeigt alle verfuegbaren Kommandos mit Beschreibung an
- [x] Die Ausgabe enthaelt: --configure, --install, --uninstall, --testrun, --help
- [x] Jedes Kommando hat eine kurze Beschreibung
- [x] Ohne gueltige Argumente wird auf `--help` hingewiesen

## Implementierung

- `Program.cs:26-49` — Help-Ausgabe mit Kommandoliste
- `ConsoleCommandLineParser.GetCommandsList()` — Liefert registrierte Kommandos
