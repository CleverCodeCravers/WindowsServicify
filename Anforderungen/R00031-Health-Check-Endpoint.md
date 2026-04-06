---
id: R00031
titel: "Health-Check HTTP-Endpoint als Differenzierungsmerkmal"
typ: Feature
status: Erledigt
prioritaet: Hoch
aufwand: Gross
erstellt: 2026-04-06
quelle: R00022-Wartung-2026-04-06 (Vorschlag 11), R00020 (Strategische Weiterentwicklung)
---

# R00031: Health-Check HTTP-Endpoint

## Beschreibung

Als erstes Differenzierungsmerkmal gegenueber WinSW, Shawl und NSSM soll WindowsServicify einen optionalen HTTP-Health-Check-Endpoint bereitstellen. Monitoring-Systeme (Prometheus, Grafana, Nagios, Uptime-Kuma) koennen so den Status des ueberwachten Prozesses abfragen. Keiner der direkten Wettbewerber bietet dieses Feature out-of-the-box.

## Ist-Zustand

- Kein Health-Check-Mechanismus
- Prozess-Status nur ueber Windows Service Manager (SCM) oder Log-Dateien einsehbar
- `Microsoft.Extensions.Hosting` ist bereits eingebunden — Health-Check-Middleware ist Teil des gleichen Oekosystems
- R00020 nennt Health-Checks als erste Differenzierungsmoeglichkeit

## Akzeptanzkriterien

### Konfiguration
- [ ] Neues optionales Feld in `ServiceConfiguration`: `HealthCheckPort` (int, nullable)
- [ ] Wenn `HealthCheckPort` nicht gesetzt: kein HTTP-Endpoint (abwaertskompatibel)
- [ ] `--configure` fragt optional nach Health-Check-Port
- [ ] Validierung: Port im gueltigen Bereich (1024–65535)

### HTTP-Endpoint
- [ ] HTTP-Server lauscht auf `http://localhost:{HealthCheckPort}/health`
- [ ] Response bei laufendem Prozess: HTTP 200 mit JSON `{"status": "healthy", "process": "running", "uptime": "HH:mm:ss"}`
- [ ] Response bei gestopptem/crashendem Prozess: HTTP 503 mit JSON `{"status": "unhealthy", "process": "stopped"}`
- [ ] Endpoint startet nur im Service-Modus und Testrun-Modus

### Monitoring-Kompatibilitaet
- [ ] Prometheus-kompatibel: `/health` Endpoint genuegt fuer Blackbox-Exporter
- [ ] JSON-Format fuer strukturierte Auswertung
- [ ] Response-Time unter 100ms

### Tests
- [ ] Unit-Tests fuer Health-Check-Logik
- [ ] Integration-Test: HTTP-Request → korrekter Status
- [ ] Test: Kein Endpoint wenn HealthCheckPort nicht gesetzt

## Umsetzung

1. `ServiceConfiguration` um `HealthCheckPort` (int?) erweitern
2. `ServiceConfigurationValidator` um Port-Validierung ergaenzen
3. `ServiceConfigurationRequester` um optionale Port-Abfrage ergaenzen
4. Neuen `HealthCheckService` erstellen:
   - Nutzt `Microsoft.AspNetCore.Builder` (Minimal API) oder `HttpListener`
   - Fragt `ProcessManager.IsCorrectlyRunning()` ab
   - Trackt Uptime seit letztem Start
5. In `Program.cs` und `WindowsBackgroundService` integrieren (nur wenn Port konfiguriert)
6. Tests schreiben

### Technische Entscheidung: ASP.NET Minimal API vs HttpListener

**Option A: Minimal API** (`Microsoft.AspNetCore.App`)
- Pro: Modernes .NET-Oekosystem, einfache Middleware-Integration, Swagger moeglich
- Contra: Groessere Abhaengigkeit, erhoehte Binary-Groesse

**Option B: HttpListener**
- Pro: Keine zusaetzlichen Abhaengigkeiten, minimal
- Contra: Low-Level, mehr Boilerplate

**Empfehlung:** HttpListener — haelt die Binary klein und vermeidet eine grosse Framework-Abhaengigkeit fuer einen einzelnen Endpoint.

## Betroffene Dateien

| Datei | Aenderung |
|-------|-----------|
| `Source/WindowsServicify/WindowsServicify.Domain/ServiceConfigurations/ServiceConfiguration.cs` | HealthCheckPort-Property |
| `Source/WindowsServicify/WindowsServicify.Domain/ServiceConfigurations/ServiceConfigurationValidator.cs` | Port-Validierung |
| `Source/WindowsServicify/WindowsServicify.Domain/ServiceConfigurations/ServiceConfigurationRequester.cs` | Port-Abfrage |
| `Source/WindowsServicify/WindowsServicify.Domain/HealthCheckService.cs` | Neu: HTTP-Endpoint |
| `Source/WindowsServicify/WindowsServicify.Domain/WindowsBackgroundService.cs` | HealthCheck-Integration |
| `Source/WindowsServicify/WindowsServicify.ConsoleApp/Program.cs` | HealthCheck im Testrun |
| `Source/WindowsServicify/WindowsServicify.Domain.Tests/HealthCheckServiceTests.cs` | Neue Tests |

## Abhaengigkeiten

- Abhaengig von: R00024 (Validierung — HealthCheckPort-Validierung baut darauf auf)
- Bezug: R00020 (Strategische Weiterentwicklung — erstes Differenzierungsmerkmal)
