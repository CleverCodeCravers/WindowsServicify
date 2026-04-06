---
id: R00027
titel: "Result-Pattern konsequent einsetzen"
typ: Verbesserung
status: Abgeschlossen
prioritaet: Mittel
aufwand: Mittel
erstellt: 2026-04-06
quelle: R00022-Wartung-2026-04-06 (Vorschlag 7)
---

# R00027: Result-Pattern konsequent einsetzen

## Beschreibung

Das Projekt nutzt `Result<T>` im `ConsoleCommandLineParser`, aber `ServiceConfigurationFileHandler.Load()` und `ServiceConfigurationValidator.Validate()` werfen Exceptions. Diese Inkonsistenz widerspricht dem eigenen Architektur-Prinzip ("Fehlerbehaftete Operationen geben Result<T> zurueck statt Exceptions zu werfen").

## Ist-Zustand

- `ConsoleCommandLineParser.Parse()` → gibt `Result<ConsoleCommandLineParameters>` zurueck ✅
- `ServiceConfigurationFileHandler.Load()` → wirft `FileNotFoundException`, `JsonException`, `ServiceConfigurationValidationException`
- `ServiceConfigurationValidator.Validate()` → wirft `ServiceConfigurationValidationException`
- `Program.cs:50,83,119,127` — ruft `Load()` ohne try/catch auf (Absturz bei fehlender/ungueltiger config.json)

## Akzeptanzkriterien

- [ ] `ServiceConfigurationFileHandler.Load()` gibt `Result<ServiceConfiguration>` zurueck
- [ ] `ServiceConfigurationValidator.Validate()` gibt `Result<ServiceConfiguration>` zurueck (Fehlerliste als Error-Message)
- [ ] `ServiceConfigurationValidationException` entfernt (nicht mehr benoetigt)
- [ ] `Program.cs` nutzt Result-Pattern fuer Config-Laden und zeigt Fehlermeldungen an
- [ ] Alle bestehenden Tests angepasst und gruen
- [ ] Neue Tests fuer Fehlerfaelle (fehlende Datei, ungueltige JSON, Validierungsfehler)

## Umsetzung

1. `ServiceConfigurationValidator.Validate()` aendern: `Result<ServiceConfiguration>` statt Exception
2. `ServiceConfigurationFileHandler.Load()` aendern: try/catch intern, `Result<ServiceConfiguration>` zurueckgeben
3. `ServiceConfigurationValidationException.cs` loeschen
4. `Program.cs` anpassen: Result pruefen, Fehlermeldung ausgeben, mit Exit-Code 1 beenden
5. Tests anpassen: Statt `Assert.Throws` → Result.IsSuccess/ErrorMessage pruefen

## Betroffene Dateien

| Datei | Aenderung |
|-------|-----------|
| `Source/WindowsServicify/WindowsServicify.Domain/ServiceConfigurations/ServiceConfigurationValidator.cs` | Return Result statt throw |
| `Source/WindowsServicify/WindowsServicify.Domain/ServiceConfigurations/ServiceConfigurationFileHandler.cs` | Return Result statt throw |
| `Source/WindowsServicify/WindowsServicify.Domain/ServiceConfigurations/ServiceConfigurationValidationException.cs` | Loeschen |
| `Source/WindowsServicify/WindowsServicify.ConsoleApp/Program.cs` | Result-Handling |
| `Source/WindowsServicify/WindowsServicify.Domain.Tests/ServiceConfigurationValidatorTests.cs` | Tests anpassen |
| `Source/WindowsServicify/WindowsServicify.Domain.Tests/ServiceConfigurationFileHandlerTests.cs` | Tests anpassen |

## Abhaengigkeiten

- Blockiert: R00028 (Exit-Codes — kann gemeinsam umgesetzt werden)
