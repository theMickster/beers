#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

"${SCRIPT_DIR}/start-api-container.sh"
trap '"${SCRIPT_DIR}/stop-api-container.sh"' EXIT

"${SCRIPT_DIR}/run-playwright-api-read.sh"
"${SCRIPT_DIR}/run-k6-api-read.sh" "${TEST_MODE:-smoke}"
