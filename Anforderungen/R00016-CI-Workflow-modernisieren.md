---
id: R00016
titel: "CI-Workflow modernisieren"
typ: Verbesserung
status: Offen
prioritaet: Mittel
aufwand: Klein
erstellt: 2026-04-03
quelle: R00001-Wartung-2026-04-03 (Vorschlag 8)
---

# R00016: CI-Workflow modernisieren

## Beschreibung

Der GitHub Actions Workflow nutzt veraltete Action-Versionen, hat keine Tests aktiviert und keine automatische Abhaengigkeitspruefung.

## Ist-Zustand

- `build.yml:23`: `actions/checkout@v2` (aktuell v4)
- `build.yml:25`: `actions/setup-dotnet@v1` (aktuell v4)
- `build.yml:45-46`: Test-Schritt auskommentiert
- Kein Dependabot/Renovate konfiguriert
- Keine Branch-Protection, keine PR-Templates, keine Issue-Templates
- Versionierung auskommentiert (Zeile 36-39)

## Akzeptanzkriterien

- [ ] `actions/checkout@v4` und `actions/setup-dotnet@v4`
- [ ] Test-Schritt aktiv: `dotnet test` im CI
- [ ] `dependabot.yml` fuer NuGet und GitHub Actions konfiguriert
- [ ] .NET SDK-Version auf 8.0.x aktualisiert (abhaengig von R00010)
- [ ] Privater NuGet-Feed entfernt (abhaengig von R00011)

## Umsetzung

1. Action-Versionen aktualisieren
2. Test-Schritt einkommentieren und anpassen
3. `.github/dependabot.yml` erstellen
4. Optional: PR-Template und Issue-Templates erstellen
5. Optional: `/erstelle-release-workflow` fuer standardisierten Release-Prozess
