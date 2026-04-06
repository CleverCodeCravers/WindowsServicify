---
id: R00025
titel: "CI-Workflow auf Release-Standard modernisieren"
typ: Verbesserung
status: Offen
prioritaet: Hoch
aufwand: Mittel
erstellt: 2026-04-06
quelle: R00022-Wartung-2026-04-06 (Vorschlag 4)
---

# R00025: CI-Workflow auf Release-Standard modernisieren

## Beschreibung

Der aktuelle `build.yml` ist ein einzelner Job ohne automatische Semver-Berechnung, ohne `workflow_dispatch`, ohne NuGet-Caching und ohne Build-Zeit-Versionierung. Der Standard-Release-Workflow sieht ein Zwei-Job-Muster mit automatischem Semver-Tagging vor.

## Ist-Zustand

- Einzelner `build`-Job fuer alles
- Kein `workflow_dispatch` (kein manueller Release moeglich)
- Kein `fetch-depth: 0` / `fetch-tags: true`
- Version nicht zur Build-Zeit gesetzt (Set-Version-Number.ps1 auskommentiert)
- Kein NuGet-Caching
- Kein Coverage-Report im CI
- Release nur bei manuellem Tag-Push

## Akzeptanzkriterien

- [ ] Zwei-Job-Muster: `prepare` (Semver-Berechnung) → `build-and-release`
- [ ] `workflow_dispatch` mit Major/Minor/Patch-Auswahl
- [ ] `fetch-depth: 0` und `fetch-tags: true` im Checkout
- [ ] Version wird zur Build-Zeit via `/p:Version=` gesetzt
- [ ] NuGet-Caching konfiguriert
- [ ] Tests werden vor Publish ausgefuehrt (bereits gegeben)
- [ ] Semver-Tag (`vX.Y.Z`) wird automatisch erstellt

## Umsetzung

1. `/erstelle-release-workflow` ausfuehren und an Projekt anpassen
2. Bestehenden `build.yml` durch neuen Standard-Workflow ersetzen
3. Sicherstellen dass Windows-only Build (`win-x64`) erhalten bleibt
4. `Compress-Archive` statt `vimtor/action-zip` fuer Windows-Kompatibilitaet pruefen

## Betroffene Dateien

| Datei | Aenderung |
|-------|-----------|
| `.github/workflows/build.yml` | Komplett ueberarbeiten |

## Abhaengigkeiten

- Abhaengig von: R00023 (TreatWarningsAsErrors — Build muss weiterhin 0-Warnungen haben)
