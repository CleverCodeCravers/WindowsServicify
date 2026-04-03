---
id: R00010
titel: ".NET 8.0 LTS Migration"
typ: Sicherheit
status: Offen
prioritaet: Hoch
aufwand: Klein
erstellt: 2026-04-03
quelle: R00001-Wartung-2026-04-03 (Vorschlag 2)
---

# R00010: .NET 8.0 LTS Migration

## Beschreibung

Das Projekt targetiert .NET 7.0, das seit Mai 2024 End-of-Life ist und keine Sicherheitsupdates mehr erhaelt.

## Ist-Zustand

- Alle drei .csproj-Dateien: `<TargetFramework>net7.0</TargetFramework>`
- Build-Warnung NETSDK1138 bei jedem Build
- CI-Workflow: `DOTNET_VERSION: "7.0.x"`
- NuGet-Pakete auf 7.0.0-Versionen

## Akzeptanzkriterien

- [ ] Alle Projekte targetieren `net8.0`
- [ ] Alle NuGet-Pakete auf .NET 8.0-kompatible Versionen aktualisiert
- [ ] CI-Workflow nutzt .NET 8.0 SDK
- [ ] Build ohne NETSDK1138-Warnung
- [ ] Alle bestehende Funktionalitaet weiterhin gegeben

## Umsetzung

1. In `WindowsServicify.ConsoleApp.csproj`, `WindowsServicify.Domain.csproj`, `WindowsServicify.Domain.Tests.csproj`: `net7.0` -> `net8.0`
2. `Microsoft.Extensions.Hosting.WindowsServices` -> `8.0.x`
3. `Microsoft.NET.Test.Sdk` -> `17.9.x`+
4. `NUnit` -> `4.x`, `NUnit3TestAdapter` -> `NUnit.Analyzers` + `NUnit4TestAdapter`
5. `build.yml`: `DOTNET_VERSION: "8.0.x"`
6. Build und Tests lokal verifizieren
