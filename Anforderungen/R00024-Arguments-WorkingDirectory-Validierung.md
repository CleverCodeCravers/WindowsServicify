---
id: R00024
titel: "Arguments- und WorkingDirectory-Validierung ergaenzen"
typ: Sicherheit
status: Offen
prioritaet: Hoch
aufwand: Klein
erstellt: 2026-04-06
quelle: R00022-Wartung-2026-04-06 (Vorschlag 3)
---

# R00024: Arguments- und WorkingDirectory-Validierung ergaenzen

## Beschreibung

`ServiceConfigurationValidator` validiert `ServiceName`, `DisplayName`, `Description` und `Command` — aber nicht `Arguments` und `WorkingDirectory`. Obwohl `ProcessManager` kein Shell nutzt (geringes Injektionsrisiko), fehlt Defense-in-Depth fuer diese beiden Felder.

## Ist-Zustand

- `ServiceConfigurationValidator.cs:13-26`: Validate() prueft 4 von 6 Feldern
- `Arguments` wird gar nicht validiert
- `WorkingDirectory` wird gar nicht validiert
- ProcessManager nutzt `Process.Start()` ohne Shell — Risiko gering, aber nicht null

## Akzeptanzkriterien

- [ ] `WorkingDirectory` wird auf Path-Traversal geprueft (`..` nicht erlaubt)
- [ ] `WorkingDirectory` wird als nicht-leerer Pflichtfeld validiert
- [ ] `Arguments` wird auf offensichtliche Injection-Muster geprueft (Backticks, `$(...)`, Pipe `|`)
- [ ] `Arguments` darf leer sein (optionales Feld)
- [ ] Bestehende 130+ Tests bleiben gruen
- [ ] Neue Tests fuer Arguments- und WorkingDirectory-Validierung

## Umsetzung

1. In `ServiceConfigurationValidator.Validate()`:
   - `ValidateWorkingDirectory()` hinzufuegen: Pflichtfeld, kein `..`, SafeNamePattern zu restriktiv — eigenes Pattern mit Pfadzeichen (`\`, `/`, `:`)
   - `ValidateArguments()` hinzufuegen: Optional, Blacklist fuer `$(`, `` ` ``, `|`, `;`
2. Tests in `ServiceConfigurationValidatorTests.cs` ergaenzen

## Betroffene Dateien

| Datei | Aenderung |
|-------|-----------|
| `Source/WindowsServicify/WindowsServicify.Domain/ServiceConfigurations/ServiceConfigurationValidator.cs` | Neue Validierungsmethoden |
| `Source/WindowsServicify/WindowsServicify.Domain.Tests/ServiceConfigurationValidatorTests.cs` | Neue Testfaelle |
