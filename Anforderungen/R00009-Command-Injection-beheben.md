---
id: R00009
titel: "Command-Injection-Schwachstellen beheben"
typ: Sicherheit
status: Offen
prioritaet: Hoch
aufwand: Klein
erstellt: 2026-04-03
quelle: R00001-Wartung-2026-04-03 (Vorschlag 1)
---

# R00009: Command-Injection-Schwachstellen beheben

## Beschreibung

ServiceName, DisplayName und Description aus `config.json` werden ohne ausreichende Validierung an `sc.exe` und PowerShell-Kommandos uebergeben. Ein boeswillig manipuliertes `config.json` koennte beliebige Kommandos ausfuehren.

## Ist-Zustand

- `LegacyWindowsServiceInstallHelper.cs:12`: ServiceName wird direkt in `sc.exe`-Argumente eingesetzt — keine Sanitisierung
- `PowerShellWindowsServiceInstallHelper.cs:7-9`: `SanitizeForPowershell()` entfernt nur `"`, Newlines und `&` — PowerShell-Injection weiterhin moeglich (`$(...)`, Backticks, Semikolons)
- `filePath` wird in keiner der beiden Implementierungen sanitisiert

## Akzeptanzkriterien

- [ ] ServiceName wird gegen Whitelist-Pattern validiert: `^[a-zA-Z0-9_\-\. ]+$`
- [ ] DisplayName und Description werden ebenso validiert
- [ ] filePath wird auf Existenz geprueft und gegen Path-Traversal gesichert
- [ ] Ungueltige Werte fuehren zu einer klaren Fehlermeldung
- [ ] Validierung erfolgt beim Laden der config.json (zentral in ServiceConfigurationFileHandler oder ServiceConfiguration)

## Umsetzung

1. Validierungsmethode in `ServiceConfiguration` oder `ServiceConfigurationFileHandler.Load()` hinzufuegen
2. Whitelist-Regex fuer ServiceName, DisplayName, Description
3. Path-Validierung fuer filePath
4. In beiden Install-Helpern sicherstellen, dass nur validierte Werte ankommen
5. Unit-Tests fuer Validierung schreiben
