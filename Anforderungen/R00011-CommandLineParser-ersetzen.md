---
id: R00011
titel: "CommandLineArgumentsParser durch oeffentliches Paket ersetzen"
typ: Verbesserung
status: Offen
prioritaet: Hoch
aufwand: Mittel
erstellt: 2026-04-03
quelle: R00001-Wartung-2026-04-03 (Vorschlag 3)
---

# R00011: CommandLineArgumentsParser durch oeffentliches Paket ersetzen

## Beschreibung

Das Projekt haengt von `CommandLineArgumentsParser 1.0.7` ab, einem privaten NuGet-Paket aus dem CleverCodeCravers GitHub Feed. Ohne Zugang zu diesem Feed ist das Projekt nicht baubar.

## Ist-Zustand

- `WindowsServicify.Domain.csproj`: `<PackageReference Include="CommandLineArgumentsParser" Version="1.0.7" />`
- Paket existiert nicht auf nuget.org
- Aufgeloeste Version (2.6.0.2) ist .NET Framework 4.x, nicht kompatibel mit .NET 7/8 (NU1701)
- CI-Workflow benoetigt `NUGET_PAT`-Secret fuer privaten Feed (build.yml:29-30)
- Build schlaegt ohne privaten Feed fehl

## Akzeptanzkriterien

- [ ] Abhaengigkeit von privatem NuGet-Feed entfernt
- [ ] Command-Line-Parsing nutzt oeffentlich verfuegbares, aktiv gepflegtes Paket
- [ ] Alle bestehenden CLI-Optionen funktionieren weiterhin: --configure, --install, --uninstall, --testrun, --legacy, --help
- [ ] Projekt ist ohne spezielle NuGet-Konfiguration baubar
- [ ] Privater NuGet-Feed-Schritt aus build.yml entfernt

## Umsetzung

**Option A: System.CommandLine** (Microsoft, Teil des .NET-Oekosystems)
- Moderner, aber noch als Preview

**Option B: CommandLineParser** (nuget.org, >100M Downloads)
- Stabil, weit verbreitet, Attribut-basiert

**Schritte:**
1. Neues Paket hinzufuegen
2. `ConsoleCommandLineParser.cs` umschreiben
3. `ConsoleCommandLineParameters.cs` ggf. anpassen (Attribute hinzufuegen)
4. `Program.cs` Aufrufe anpassen
5. Privaten NuGet-Feed aus build.yml entfernen (Zeile 29-30)
6. Tests fuer CLI-Parsing schreiben
