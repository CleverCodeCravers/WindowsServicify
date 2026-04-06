#!/bin/bash
# Tests fuer calculate-next-version.sh
# Ausfuehrung: bash Scripts/calculate-next-version.tests.sh

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

assert_error() {
  local test_name="$1"
  shift
  if output=$("$SCRIPT" "$@" 2>/dev/null); then
    echo "  FAIL: $test_name (expected error, got success)"
    FAIL=$((FAIL + 1))
  else
    echo "  PASS: $test_name"
    PASS=$((PASS + 1))
  fi
}

echo "=== Semver-Berechnung: Patch-Bump ==="

output=$(bash "$SCRIPT" "v1.8.0" "patch")
assert_output "v1.8.0 + patch = v1.8.1" "1.8.1" "v1.8.1" "$output"

output=$(bash "$SCRIPT" "v1.0.0" "patch")
assert_output "v1.0.0 + patch = v1.0.1" "1.0.1" "v1.0.1" "$output"

output=$(bash "$SCRIPT" "v0.0.0" "patch")
assert_output "v0.0.0 + patch = v0.0.1" "0.0.1" "v0.0.1" "$output"

output=$(bash "$SCRIPT" "v2.5.9" "patch")
assert_output "v2.5.9 + patch = v2.5.10" "2.5.10" "v2.5.10" "$output"

echo ""
echo "=== Semver-Berechnung: Minor-Bump ==="

output=$(bash "$SCRIPT" "v1.8.0" "minor")
assert_output "v1.8.0 + minor = v1.9.0" "1.9.0" "v1.9.0" "$output"

output=$(bash "$SCRIPT" "v1.8.5" "minor")
assert_output "v1.8.5 + minor = v1.9.0" "1.9.0" "v1.9.0" "$output"

output=$(bash "$SCRIPT" "v0.0.0" "minor")
assert_output "v0.0.0 + minor = v0.1.0" "0.1.0" "v0.1.0" "$output"

echo ""
echo "=== Semver-Berechnung: Major-Bump ==="

output=$(bash "$SCRIPT" "v1.8.0" "major")
assert_output "v1.8.0 + major = v2.0.0" "2.0.0" "v2.0.0" "$output"

output=$(bash "$SCRIPT" "v0.9.9" "major")
assert_output "v0.9.9 + major = v1.0.0" "1.0.0" "v1.0.0" "$output"

output=$(bash "$SCRIPT" "v3.0.0" "major")
assert_output "v3.0.0 + major = v4.0.0" "4.0.0" "v4.0.0" "$output"

echo ""
echo "=== Semver-Berechnung: Zwei-Segment-Tags (v1.8 -> v1.8.0) ==="

output=$(bash "$SCRIPT" "v1.8" "patch")
assert_output "v1.8 + patch = v1.8.1" "1.8.1" "v1.8.1" "$output"

output=$(bash "$SCRIPT" "v1.8" "minor")
assert_output "v1.8 + minor = v1.9.0" "1.9.0" "v1.9.0" "$output"

output=$(bash "$SCRIPT" "v1.8" "major")
assert_output "v1.8 + major = v2.0.0" "2.0.0" "v2.0.0" "$output"

echo ""
echo "=== Semver-Berechnung: Leerer Tag (kein vorheriger Release) ==="

output=$(bash "$SCRIPT" "" "patch")
assert_output "'' + patch = v0.0.1" "0.0.1" "v0.0.1" "$output"

output=$(bash "$SCRIPT" "" "minor")
assert_output "'' + minor = v0.1.0" "0.1.0" "v0.1.0" "$output"

output=$(bash "$SCRIPT" "" "major")
assert_output "'' + major = v1.0.0" "1.0.0" "v1.0.0" "$output"

echo ""
echo "=== Semver-Berechnung: Default Bump-Typ (patch) ==="

output=$(bash "$SCRIPT" "v1.0.0")
assert_output "v1.0.0 + default = v1.0.1" "1.0.1" "v1.0.1" "$output"

echo ""
echo "=== Semver-Berechnung: Ungueltiger Bump-Typ ==="

assert_error "Ungueltiger Bump-Typ 'invalid'" "v1.0.0" "invalid"

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
