---
id: R00004
titel: "Service-Deinstallation (--uninstall)"
typ: Feature
status: Implementiert
erstellt: 2026-04-03
---

# R00004: Service-Deinstallation (--uninstall)

## Beschreibung

Als Benutzer moechte ich einen zuvor installierten Windows-Service mit einem Kommando entfernen koennen.

## Akzeptanzkriterien

- [x] `WindowsServicify.exe --uninstall` entfernt den in `config.json` benannten Windows-Service
- [x] Standard-Methode: PowerShell (`Remove-Service`)
- [x] Legacy-Modus: `--uninstall --legacy` nutzt `sc.exe delete`
- [x] Nach erfolgreicher Deinstallation wird eine Erfolgsmeldung angezeigt
- [x] Bei Fehler wird die Fehlermeldung ausgegeben

## Implementierung

- `PowerShellWindowsServiceInstallHelper.RemoveService()` — Deinstallation via PowerShell
- `LegacyWindowsServiceInstallHelper.RemoveService()` — Deinstallation via sc.exe
