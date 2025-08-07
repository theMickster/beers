#!/usr/bin/env bash

set -euo pipefail
source "$(dirname "$0")/common.sh"

pushd "${ROOT_DIR}/tests/api-read" >/dev/null
npm ci
BASE_URL="${BASE_URL}" npm run api-read:playwright${CI:+:ci}
popd >/dev/null
