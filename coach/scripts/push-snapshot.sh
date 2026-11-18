#!/usr/bin/env bash
# Push the nightly results snapshot from the internal CloverleafTrack app to Cloverleaf Coach. BRAIN.md [C133].
# Runs nightly on svp01docker01 (cron 03:15, after the 02:30 image deploy). Data only flows one way: home -> Cloudflare.
#
# Environment:
#   COACH_SNAPSHOT_TOKEN    required; must match the Worker secret SNAPSHOT_TOKEN
#   SNAPSHOT_SOURCE         default http://localhost:8081/Admin/CoachSnapshot (the container on svp01docker01)
#   COACH_URL               default https://coach.cloverleaftrack.com
#   CF_ACCESS_CLIENT_ID / CF_ACCESS_CLIENT_SECRET
#                           optional Cloudflare Access service token, only needed if /api/snapshot is not
#                           bypassed in the Access application
set -euo pipefail

: "${COACH_SNAPSHOT_TOKEN:?set COACH_SNAPSHOT_TOKEN}"
SOURCE="${SNAPSHOT_SOURCE:-http://localhost:8081/Admin/CoachSnapshot}"
TARGET="${COACH_URL:-https://coach.cloverleaftrack.com}/api/snapshot"

tmp="$(mktemp)"
trap 'rm -f "$tmp"' EXIT

curl -fsS --max-time 120 "$SOURCE" -o "$tmp"

access_headers=()
if [[ -n "${CF_ACCESS_CLIENT_ID:-}" ]]; then
  access_headers=(-H "CF-Access-Client-Id: $CF_ACCESS_CLIENT_ID" -H "CF-Access-Client-Secret: $CF_ACCESS_CLIENT_SECRET")
fi

curl -fsS --max-time 120 -X POST "$TARGET" \
  -H "Authorization: Bearer $COACH_SNAPSHOT_TOKEN" \
  -H "Content-Type: application/json" \
  ${access_headers[@]+"${access_headers[@]}"} \
  --data-binary "@$tmp"
echo
