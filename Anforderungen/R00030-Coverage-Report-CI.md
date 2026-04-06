---
id: R00030
titel: "Coverage-Report im CI-Workflow"
typ: Verbesserung
status: Abgeschlossen
prioritaet: Mittel
aufwand: Klein
erstellt: 2026-04-06
quelle: R00022-Wartung-2026-04-06 (Vorschlag 10)
---

# R00030: Coverage-Report im CI-Workflow

## Beschreibung

Code-Coverage wird nur lokal gemessen. Der CI-Workflow fuehrt Tests ohne Coverage-Erfassung aus. Coverage-Daten sollten im CI erfasst und als Artefakt verfuegbar sein.

## Ist-Zustand

- `build.yml:43-44`: `dotnet test` ohne `--collect:"XPlat Code Coverage"`
- `coverlet.runsettings` vorhanden aber nur lokal genutzt
- Aktuelle Coverage: 71.7% Line, 74.6% Branch (bei Ausschluss der Infrastruktur-Klassen)

## Akzeptanzkriterien

- [ ] CI-Workflow sammelt Coverage-Daten via `--collect:"XPlat Code Coverage"`
- [ ] Coverage-Report wird als CI-Artefakt hochgeladen
- [ ] `coverlet.runsettings` wird im CI genutzt (konsistente Ausschluesse)
- [ ] Optional: Coverage-Badge im README

## Umsetzung

1. Test-Step in `build.yml` aendern:
   ```yaml
   - name: Test
     run: dotnet test --configuration Release --no-build --verbosity normal --collect:"XPlat Code Coverage" --settings coverlet.runsettings
     working-directory: Source/${{ env.SOLUTION_DIR }}
   ```
2. Artefakt-Upload hinzufuegen:
   ```yaml
   - uses: actions/upload-artifact@v4
     with:
       name: coverage-report
       path: Source/${{ env.SOLUTION_DIR }}/**/coverage.cobertura.xml
   ```
3. Optional: `reportgenerator` fuer HTML-Report integrieren
4. Optional: Codecov-Integration fuer Badge

## Betroffene Dateien

| Datei | Aenderung |
|-------|-----------|
| `.github/workflows/build.yml` | Test-Step erweitern, Artefakt-Upload |
