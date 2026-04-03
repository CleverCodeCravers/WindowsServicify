---
id: R00015
titel: ".gitignore vervollstaendigen"
typ: Verbesserung
status: Offen
prioritaet: Mittel
aufwand: Klein
erstellt: 2026-04-03
quelle: R00001-Wartung-2026-04-03 (Vorschlag 7)
---

# R00015: .gitignore vervollstaendigen

## Beschreibung

Die `.gitignore` enthaelt nur einen einzigen Eintrag (`/Source/WindowsServicify/.idea`). Standard-.NET-Patterns fehlen vollstaendig.

## Ist-Zustand

```
/Source/WindowsServicify/.idea
```

## Akzeptanzkriterien

- [ ] .gitignore enthaelt Standard-.NET-Patterns: `bin/`, `obj/`, `*.user`, `*.suo`, `.vs/`, `*.DotSettings.user`, `TestResults/`
- [ ] Bestehender `.idea`-Eintrag bleibt erhalten
- [ ] Keine bereits getrackten Dateien werden versehentlich ignoriert

## Umsetzung

1. `dotnet new gitignore` im Root ausfuehren
2. Bestehenden `.idea`-Eintrag in die generierte Datei uebernehmen
3. Pruefen ob bereits getrackte Dateien betroffen sind
