---
id: R00033
titel: "README modernisieren"
typ: Verbesserung
status: Abgeschlossen
prioritaet: Niedrig
aufwand: Klein
erstellt: 2026-04-06
quelle: R00022-Wartung-2026-04-06 (Vorschlag 14)
---

# R00033: README modernisieren

## Beschreibung

Die README.md beschreibt nur die Endnutzer-Sicht (Usage, Config, Logging). Es fehlen CI-Badge, Installationsanleitung, Vergleich mit Alternativen, Contributing-Hinweis und Development-Setup.

## Ist-Zustand

- README.md: 70 Zeilen, nur Usage-Dokumentation
- Kein CI-Badge
- Keine Installationsanleitung (wie kommt man an die .exe?)
- Kein Vergleich mit Alternativen (WinSW, Shawl, NSSM)
- Kein Development-Setup (`dotnet build`, `dotnet test`)
- Kein Contributing-Verweis

## Akzeptanzkriterien

- [ ] CI-Badge am Anfang der README
- [ ] Installations-Sektion (GitHub Release Download-Link)
- [ ] Development-Setup Sektion (`dotnet build`, `dotnet test`, .NET 10.0 Voraussetzung)
- [ ] Optional: Kurzvergleich mit Alternativen oder Verweis auf R00020
- [ ] Optional: Contributing-Verweis

## Umsetzung

1. CI-Badge einfuegen: `![Build](https://github.com/CleverCodeCravers/WindowsServicify/actions/workflows/build.yml/badge.svg)`
2. Neue Sektion "Installation" mit Link zu GitHub Releases
3. Neue Sektion "Development" mit Build/Test-Befehlen und .NET 10.0 Voraussetzung
4. Optional: Sektion "Alternatives" mit Kurzverweis auf WinSW, Shawl

## Betroffene Dateien

| Datei | Aenderung |
|-------|-----------|
| `README.md` | Sektionen ergaenzen |
