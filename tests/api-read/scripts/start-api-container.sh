#!/usr/bin/env bash

set -euo pipefail
source "$(dirname "$0")/common.sh"

require_env "KeyVault__VaultUri"
require_env "AutoMapperLicenseKey"
require_env "AZURE_TENANT_ID"
require_env "AZURE_CLIENT_ID"
require_env "AZURE_CLIENT_SECRET"

pushd "${ROOT_DIR}" >/dev/null
compose_cmd up -d --build beers-api
wait_for_api
popd >/dev/null
