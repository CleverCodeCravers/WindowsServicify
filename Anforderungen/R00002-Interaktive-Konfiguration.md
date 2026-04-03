---
id: R00002
titel: "Interaktive Konfiguration (--configure)"
typ: Feature
status: Implementiert
erstellt: 2026-04-03
---

# R00002: Interaktive Konfiguration (--configure)

## Beschreibung

Als Benutzer moechte ich WindowsServicify interaktiv konfigurieren koennen, damit ich einen Windows-Service ohne manuelle JSON-Bearbeitung einrichten kann.

## Akzeptanzkriterien

- [x] `WindowsServicify.exe --configure` startet eine interaktive Abfrage
- [x] Folgende Werte werden abgefragt:
  - Service-Name (Pflicht)
  - Display-Name (Pflicht)
  - Service-Beschreibung (Optional)
  - Auszufuehrendes Kommando (Pflicht)
  - Kommando-Argumente (Optional)
  - Arbeitsverzeichnis (Pflicht)
- [x] Pflichtfelder koennen nicht leer gelassen werden
- [x] Die Werte werden als `config.json` im Verzeichnis der EXE gespeichert
- [x] Die config.json ist im JSON-Format mit eingerueckter Formatierung

## Implementierung

- `ServiceConfigurationRequester.cs` — Interaktive Abfrage
- `ConsoleInput.cs` — Eingabe-Helper mit Required-Validierung
- `ServiceConfigurationFileHandler.cs` — JSON-Serialisierung/Deserialisierung
- `ServiceConfiguration.cs` — Record-Type fuer Konfigurationsdaten
