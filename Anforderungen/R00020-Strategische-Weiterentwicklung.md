---
id: R00020
titel: "Strategische Weiterentwicklung — Differenzierung gegenueber NSSM/WinSW"
typ: Strategie
status: Entschieden
prioritaet: Hoch
aufwand: Gross
erstellt: 2026-04-03
entscheidung: Weiterentwickeln
quelle: R00001-Wartung-2026-04-03 (Vorschlag 12)
---

# R00020: Strategische Weiterentwicklung

## Entscheidung

**Weiterentwickeln** (entschieden am 2026-04-03).

## Kontext

Das Projekt ist seit 3+ Jahren inaktiv und hat keine externen Nutzer. Reifere Alternativen existieren (NSSM, WinSW, shawl). Trotzdem wird die Weiterentwicklung gewaehlt.

## Marktanalyse

| Alternative | Status | Staerken |
|-------------|--------|----------|
| **NSSM** | Effektiv unmaintained (letztes Release 2014) | Simpel, kein Build noetig, weit verbreitet |
| **WinSW** | Aktiv, ~12k GitHub Stars | XML/YAML-Config, breite Testabdeckung |
| **shawl** | Aktiv, ~2k GitHub Stars | Rust-basiert, leichtgewichtig |
| **Topshelf** | Wartungsmodus | .NET-basiert, aber fuer .NET Framework |

## Differenzierungsmoeglichkeiten

Um gegen WinSW und shawl zu bestehen, braucht WindowsServicify Alleinstellungsmerkmale:

1. **.NET-native Integration** — Bereits gegeben durch `Microsoft.Extensions.Hosting`. Ausbaubar durch Health-Check-Endpoints, Metrics, strukturiertes Logging
2. **Moderne Config** — YAML-Support oder TOML statt JSON, Environment-Variable-Substitution
3. **Health-Checks** — HTTP-Endpoint fuer Monitoring-Systeme (Prometheus, Grafana)
4. **Web-Dashboard** — Kleines lokales Web-UI fuer Status, Logs, Restart
5. **Multi-Service** — Mehrere Prozesse mit einer Instanz ueberwachen
6. **KI-Integration** — Natuerlichsprachliche Konfiguration, intelligente Restart-Strategien

## Empfohlene Reihenfolge

1. Erst technische Schulden abbauen (R00009-R00016)
2. Dann Differenzierungs-Features planen und umsetzen
3. README und Dokumentation auf Wettbewerbsvorteile ausrichten
