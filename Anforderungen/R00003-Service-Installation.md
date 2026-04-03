---
id: R00003
titel: "Service-Installation (--install)"
typ: Feature
status: Implementiert
erstellt: 2026-04-03
---

# R00003: Service-Installation (--install)

## Beschreibung

Als Benutzer moechte ich mit einem Kommando einen Windows-Service installieren koennen, der die konfigurierte Anwendung als Dienst registriert.

## Akzeptanzkriterien

- [x] `WindowsServicify.exe --install` registriert einen neuen Windows-Service
- [x] Der Service wird basierend auf den Werten aus `config.json` erstellt
- [x] Standard-Installationsmethode: PowerShell (`New-Service`)
- [x] Legacy-Modus verfuegbar: `--install --legacy` nutzt `sc.exe`
- [x] Nach erfolgreicher Installation wird eine Erfolgsmeldung mit Hinweis auf services.msc angezeigt
- [x] Bei Fehler wird die Fehlermeldung ausgegeben

## Implementierung

- `PowerShellWindowsServiceInstallHelper.cs` — Installation via PowerShell
- `LegacyWindowsServiceInstallHelper.cs` — Installation via sc.exe
- `WindowsServiceInstallHelperFactory.cs` — Factory fuer Legacy/PowerShell-Auswahl
- `IWindowsServiceInstallHelper.cs` — Interface
