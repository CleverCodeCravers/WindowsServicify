---
id: R00032
titel: "Documentation-Verzeichnis bereinigen"
typ: Verbesserung
status: Offen
prioritaet: Niedrig
aufwand: Klein
erstellt: 2026-04-06
quelle: R00022-Wartung-2026-04-06 (Vorschlag 13)
---

# R00032: Documentation-Verzeichnis bereinigen

## Beschreibung

Das `Documentation/`-Verzeichnis enthaelt veraltete Platzhalter-Dateien. `requirements.md` ist leer ("put the requirements here") — Anforderungen werden laengst in `Anforderungen/` gepflegt. `prepared-workflows/` enthaelt veraltete Workflow-Vorlagen.

## Ist-Zustand

- `Documentation/requirements.md`: Leerer Platzhalter
- `Documentation/prepared-workflows/`: Veraltete Workflow-Vorlagen (dotnet.yml, sonar.yml)
- Anforderungen werden in `Anforderungen/` gepflegt (21+ Dateien)
- CI-Workflow ist in `.github/workflows/build.yml`

## Akzeptanzkriterien

- [ ] `Documentation/requirements.md` entfernt
- [ ] `Documentation/prepared-workflows/` entfernt (falls nicht mehr referenziert)
- [ ] Falls `Documentation/` danach leer: komplett entfernen
- [ ] Keine Referenzen auf entfernte Dateien in README oder CLAUDE.md

## Umsetzung

1. Pruefen ob `Documentation/`-Inhalte irgendwo referenziert werden
2. `Documentation/requirements.md` loeschen
3. `Documentation/prepared-workflows/` loeschen
4. Falls leer: `Documentation/` loeschen

## Betroffene Dateien

| Datei | Aenderung |
|-------|-----------|
| `Documentation/requirements.md` | Loeschen |
| `Documentation/prepared-workflows/` | Loeschen |
| `Documentation/` | Ggf. loeschen |
