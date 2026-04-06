#!/bin/bash
# Integrationstests fuer calculate-next-version.sh
# Testet das Skript mit realistischen Szenarien, die den tatsaechlichen
# Einsatz im CI-Workflow widerspiegeln.
#
# Ausfuehrung: bash Scripts/calculate-next-version.integration-tests.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SCRIPT="$SCRIPT_DIR/calculate-next-version.sh"
PASS=0
FAIL=0

assert_output() {
  local test_name="$1"
  local expected_version="$2"
  local expected_tag="$3"
  local actual_output="$4"

  local actual_version
  actual_version=$(echo "$actual_output" | head -1)
  local actual_tag
  actual_tag=$(echo "$actual_output" | tail -1)

  if [ "$actual_version" = "$expected_version" ] && [ "$actual_tag" = "$expected_tag" ]; then
    echo "  PASS: $test_name"
    PASS=$((PASS + 1))
  else
    echo "  FAIL: $test_name"
    echo "    Expected: version=$expected_version tag=$expected_tag"
    echo "    Actual:   version=$actual_version tag=$actual_tag"
    FAIL=$((FAIL + 1))
  fi
}

# ──────────────────────────────────────────────
# Szenario 1: Simuliert den echten Workflow --
# git tag Ausgabe wird als Input verwendet
# ──────────────────────────────────────────────
echo "=== Integration: Simulierter Workflow mit echten Tag-Formaten ==="

# Simuliert: Repository mit Tags v1.0, v1.1, v1.8 (wie im WindowsServicify-Projekt)
# git tag --list 'v*' --sort=-version:refname | head -1 wuerde "v1.8" liefern
SIMULATED_LATEST="v1.8"
output=$(bash "$SCRIPT" "$SIMULATED_LATEST" "patch")
assert_output "WindowsServicify-Realfall: v1.8 + patch = v1.8.1" "1.8.1" "v1.8.1" "$output"

output=$(bash "$SCRIPT" "$SIMULATED_LATEST" "minor")
assert_output "WindowsServicify-Realfall: v1.8 + minor = v1.9.0" "1.9.0" "v1.9.0" "$output"

output=$(bash "$SCRIPT" "$SIMULATED_LATEST" "major")
assert_output "WindowsServicify-Realfall: v1.8 + major = v2.0.0" "2.0.0" "v2.0.0" "$output"

echo ""

# ──────────────────────────────────────────────
# Szenario 2: Sequentielle Releases
# Simuliert mehrere aufeinanderfolgende Releases
# ──────────────────────────────────────────────
echo "=== Integration: Sequentielle Release-Kette ==="

current_tag="v1.8"

output=$(bash "$SCRIPT" "$current_tag" "patch")
new_tag=$(echo "$output" | tail -1)
assert_output "Release 1: v1.8 + patch = v1.8.1" "1.8.1" "v1.8.1" "$output"

output=$(bash "$SCRIPT" "$new_tag" "patch")
assert_output "Release 2: v1.8.1 + patch = v1.8.2" "1.8.2" "v1.8.2" "$output"
new_tag=$(echo "$output" | tail -1)

output=$(bash "$SCRIPT" "$new_tag" "minor")
assert_output "Release 3: v1.8.2 + minor = v1.9.0" "1.9.0" "v1.9.0" "$output"
new_tag=$(echo "$output" | tail -1)

output=$(bash "$SCRIPT" "$new_tag" "patch")
assert_output "Release 4: v1.9.0 + patch = v1.9.1" "1.9.1" "v1.9.1" "$output"
new_tag=$(echo "$output" | tail -1)

output=$(bash "$SCRIPT" "$new_tag" "major")
assert_output "Release 5: v1.9.1 + major = v2.0.0" "2.0.0" "v2.0.0" "$output"

echo ""

# ──────────────────────────────────────────────
# Szenario 3: Erstmalige Releases (Greenfield-Projekt)
# ──────────────────────────────────────────────
echo "=== Integration: Erstmalige Release-Kette (Greenfield) ==="

output=$(bash "$SCRIPT" "" "minor")
assert_output "Erster Release: '' + minor = v0.1.0" "0.1.0" "v0.1.0" "$output"
new_tag=$(echo "$output" | tail -1)

output=$(bash "$SCRIPT" "$new_tag" "patch")
assert_output "Zweiter Release: v0.1.0 + patch = v0.1.1" "0.1.1" "v0.1.1" "$output"
new_tag=$(echo "$output" | tail -1)

output=$(bash "$SCRIPT" "$new_tag" "major")
assert_output "Dritter Release: v0.1.1 + major = v1.0.0" "1.0.0" "v1.0.0" "$output"

echo ""

# ──────────────────────────────────────────────
# Szenario 4: Hohe Versionsnummern
# ──────────────────────────────────────────────
echo "=== Integration: Hohe Versionsnummern ==="

output=$(bash "$SCRIPT" "v10.20.30" "patch")
assert_output "Hohe Version: v10.20.30 + patch = v10.20.31" "10.20.31" "v10.20.31" "$output"

output=$(bash "$SCRIPT" "v99.99.99" "minor")
assert_output "Grenzwert: v99.99.99 + minor = v99.100.0" "99.100.0" "v99.100.0" "$output"

output=$(bash "$SCRIPT" "v99.99.99" "major")
assert_output "Grenzwert: v99.99.99 + major = v100.0.0" "100.0.0" "v100.0.0" "$output"

echo ""

# ──────────────────────────────────────────────
# Szenario 5: Version wird korrekt als dotnet-Version formatiert
# Stellt sicher dass die Version im Format X.Y.Z ist (fuer /p:Version=)
# ──────────────────────────────────────────────
echo "=== Integration: dotnet-Version-Format-Validierung ==="

output=$(bash "$SCRIPT" "v1.8" "patch")
version=$(echo "$output" | head -1)
if [[ "$version" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
  echo "  PASS: Version '$version' hat korrektes X.Y.Z Format"
  PASS=$((PASS + 1))
else
  echo "  FAIL: Version '$version' hat KEIN korrektes X.Y.Z Format"
  FAIL=$((FAIL + 1))
fi

tag=$(echo "$output" | tail -1)
if [[ "$tag" =~ ^v[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
  echo "  PASS: Tag '$tag' hat korrektes vX.Y.Z Format"
  PASS=$((PASS + 1))
else
  echo "  FAIL: Tag '$tag' hat KEIN korrektes vX.Y.Z Format"
  FAIL=$((FAIL + 1))
fi

echo ""
echo "=== Ergebnis ==="
echo "  Bestanden: $PASS"
echo "  Fehlgeschlagen: $FAIL"
echo ""

if [ "$FAIL" -gt 0 ]; then
  echo "TESTS FEHLGESCHLAGEN"
  exit 1
else
  echo "ALLE TESTS BESTANDEN"
  exit 0
fi
