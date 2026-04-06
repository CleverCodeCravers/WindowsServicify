# CLAUDE.md -- Projektkontext fuer AI-Assistenten

## Ueberblick

**WindowsServicify** ist ein .NET 10.0 CLI-Tool, das beliebige Skripte und Executables als Windows-Service wrappen kann. Es konfiguriert, installiert und ueberwacht Prozesse als Windows-Dienste mit automatischem Restart und Logging.

> **Windows-only** -- Dieses Projekt laeuft ausschliesslich unter Windows. Es nutzt Windows-spezifische APIs (`Microsoft.Extensions.Hosting.WindowsServices`) und Windows-Service-Mechanismen (sc.exe / PowerShell `New-Service`). Cross-Platform-Kompatibilitaet ist nicht vorgesehen.

## Build und Test

```bash
# Solution-Pfad
Source/WindowsServicify/WindowsServicify.sln

# Bauen
dotnet build Source/WindowsServicify/WindowsServicify.sln

# Tests ausfuehren
dotnet test Source/WindowsServicify/WindowsServicify.sln
```

## Projektstruktur

```
Source/WindowsServicify/
  WindowsServicify.sln                        # Solution-Datei
  WindowsServicify.ConsoleApp/                # Entry Point -- CLI-Host, Program.cs
  WindowsServicify.Domain/                    # Business Logic -- Services, Prozess-Management, Konfiguration
  WindowsServicify.Domain.Tests/              # Unit-Tests (NUnit 4.x)
  WindowsServicify.Domain.IntegrationTests/   # Integration-Tests (NUnit 4.x)
```

| Projekt | Rolle |
|---------|-------|
| **ConsoleApp** | Entry Point und CLI-Host. Parst Kommandozeilen-Argumente, startet den Windows-Service-Host oder fuehrt Konfiguration/Installation aus. |
| **Domain** | Gesamte Business Logic. Prozess-Management, Service-Konfiguration, Logging, Windows-Service-Installation. |
| **Domain.Tests** | Unit-Tests fuer die Domain-Logik. Nutzt `InternalsVisibleTo` fuer Zugriff auf interne Klassen. |
| **Domain.IntegrationTests** | Integration-Tests fuer Prozess-Lifecycle, BackgroundService und Configure/Testrun-Flow. Nutzt echte Prozesse und temporaere Verzeichnisse. |

### Wichtige Domain-Klassen

- `ConsoleCommandLineParser` -- Parst CLI-Argumente (`--configure`, `--install`, `--uninstall`, `--testrun`, `--help`)
- `WindowsBackgroundService` -- Der eigentliche Windows-Service, der den konfigurierten Prozess ueberwacht. Nutzt `IProcessExitHandler` fuer testbaren Exit.
- `ProcessManager` -- Startet und ueberwacht den Kindprozess, faengt stdout/stderr ab
- `ProcessLogger` -- Schreibt Prozess-Ausgaben in tagesbasierte Log-Dateien (7 Tage Rotation)
- `ServiceConfigurations/` -- Konfigurationsmodell, Validierung, Datei-I/O, interaktive Abfrage
- `Result<T>` -- Generisches Result-Pattern fuer fehlerbehandelte Rueckgabewerte

## Konventionen

- **Target Framework**: .NET 10.0 (LTS)
- **Nullable Reference Types**: Aktiviert (`<Nullable>enable</Nullable>`) in allen Projekten
- **Record-Types fuer DTOs**: Immutable Datenklassen werden als `record` definiert (z.B. `ServiceConfiguration`, `ConsoleCommandLineParameters`, `CommandDefinition`)
- **Result-Pattern**: Fehlerbehaftete Operationen geben `Result<T>` zurueck statt Exceptions zu werfen
- **Test-Framework**: NUnit 4.x mit NUnit3TestAdapter, Coverage via coverlet.collector
- **Namensraum**: `WindowsServicify.Domain` / `WindowsServicify.ConsoleApp`
- **ImplicitUsings**: Aktiviert in allen Projekten

## Anforderungen

Funktionale und technische Anforderungen werden im Verzeichnis `Anforderungen/` als Markdown-Dateien gepflegt. Jede Anforderung hat eine ID (z.B. `R00017`), Akzeptanzkriterien und Umsetzungshinweise. User Stories liegen in `user-stories/`.
