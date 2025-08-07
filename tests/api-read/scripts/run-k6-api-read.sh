#!/usr/bin/env bash

set -euo pipefail
source "$(dirname "$0")/common.sh"

MODE="${1:-${TEST_MODE:-smoke}}"
SCRIPT_PATH="${ROOT_DIR}/tests/api-read/k6/dist/api-read.js"

pushd "${ROOT_DIR}/tests/api-read" >/dev/null
if [[ ! -d node_modules ]]; then
  npm ci >/dev/null
fi
npm run api-read:k6:build >/dev/null
popd >/dev/null

if command -v k6 >/dev/null 2>&1; then
  BASE_URL="${BASE_URL}" TEST_MODE="${MODE}" k6 run "${SCRIPT_PATH}"
  exit 0
fi

echo "k6 binary not found; using grafana/k6 container"

docker run --rm \
  --add-host=host.docker.internal:host-gateway \
  -e BASE_URL="${BASE_URL/localhost/host.docker.internal}" \
  -e TEST_MODE="${MODE}" \
  -v "${ROOT_DIR}/tests/api-read/k6/dist:/scripts" \
  grafana/k6:latest run /scripts/api-read.js
