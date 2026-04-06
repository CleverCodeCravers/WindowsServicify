---
id: R00021
titel: "Migration von .NET 8.0 auf .NET 10.0"
typ: Wartung
status: Abgeschlossen
prioritaet: Hoch
aufwand: Klein
erstellt: 2026-04-06
quelle: "GitHub Issue: #27"
---

# R00021: Migration von .NET 8.0 auf .NET 10.0

## Beschreibung

Das Projekt targetiert .NET 8.0. Laut Wartungsliste ist der Ziel-TechStack .NET 10.0. Die Migration stellt sicher, dass das Projekt auf dem aktuellen LTS-Framework laeuft.

## Ist-Zustand

- Alle drei .csproj-Dateien: `<TargetFramework>net8.0</TargetFramework>`
- CI-Workflow: `DOTNET_VERSION: "8.0.x"`
- `Microsoft.Extensions.Hosting.WindowsServices` bereits auf Version 10.0.5 (schon .NET 10-kompatibel)
- Alle NuGet-Pakete auf aktuellem Stand (kein Update verfuegbar)
- 130 Tests gruen, 0 Warnungen

## Akzeptanzkriterien

- [ ] Alle Projekte targetieren `net10.0`
- [ ] CI-Workflow nutzt .NET 10.0 SDK
- [ ] Build ohne Warnungen
- [ ] Alle 130+ bestehenden Tests gruen
- [ ] Keine Deprecation-Warnungen

## Umsetzung

1. In `WindowsServicify.ConsoleApp.csproj`: `net8.0` -> `net10.0`
2. In `WindowsServicify.Domain.csproj`: `net8.0` -> `net10.0`
3. In `WindowsServicify.Domain.Tests.csproj`: `net8.0` -> `net10.0`
4. In `.github/workflows/build.yml`: `DOTNET_VERSION: "8.0.x"` -> `DOTNET_VERSION: "10.0.x"`
5. Build und Tests lokal verifizieren

## Betroffene Dateien

- `Source/WindowsServicify/WindowsServicify.ConsoleApp/WindowsServicify.ConsoleApp.csproj`
- `Source/WindowsServicify/WindowsServicify.Domain/WindowsServicify.Domain.csproj`
- `Source/WindowsServicify/WindowsServicify.Domain.Tests/WindowsServicify.Domain.Tests.csproj`
- `.github/workflows/build.yml`
