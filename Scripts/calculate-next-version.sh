#!/bin/bash
# calculate-next-version.sh
# Berechnet die naechste Semver-Version basierend auf dem aktuellen Tag und Bump-Typ.
#
# Usage: calculate-next-version.sh <latest-tag> <bump-type>
#   latest-tag:  z.B. "v1.8", "v1.8.0", "v2.0.1" oder "" (kein Tag)
#   bump-type:   "major", "minor" oder "patch"
#
# Output (stdout): Zwei Zeilen:
#   Zeile 1: Version ohne Prefix (z.B. "1.9.0")
#   Zeile 2: Tag mit Prefix (z.B. "v1.9.0")

set -euo pipefail

LATEST_TAG="${1:-}"
BUMP="${2:-patch}"

if [ -z "$LATEST_TAG" ]; then
  LATEST_TAG="v0.0.0"
fi

# Tag-Prefix entfernen
VERSION="${LATEST_TAG#v}"

# Semver-Teile extrahieren (v1.8 -> 1.8.0, v1.8.3 -> 1.8.3)
IFS='.' read -r MAJOR MINOR PATCH <<< "$VERSION"
MAJOR="${MAJOR:-0}"
MINOR="${MINOR:-0}"
PATCH="${PATCH:-0}"

# Bump-Typ anwenden
case "$BUMP" in
  major)
    MAJOR=$((MAJOR + 1))
    MINOR=0
    PATCH=0
    ;;
  minor)
    MINOR=$((MINOR + 1))
    PATCH=0
    ;;
  patch)
    PATCH=$((PATCH + 1))
    ;;
  *)
    echo "ERROR: Unknown bump type '$BUMP'. Use major, minor, or patch." >&2
    exit 1
    ;;
esac

NEW_VERSION="$MAJOR.$MINOR.$PATCH"
NEW_TAG="v$NEW_VERSION"

echo "$NEW_VERSION"
echo "$NEW_TAG"
