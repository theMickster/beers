#!/usr/bin/env bash
set -euo pipefail

source "$(dirname "$0")/common.sh"
init_context "${1:-}"

az bicep build --file "$COSMOS_BICEP_TEMPLATE" >/dev/null

az deployment group validate \
  --subscription "$AZURE_SUBSCRIPTION_ID" \
  --resource-group "$COSMOS_RESOURCE_GROUP" \
  --template-file "$COSMOS_BICEP_TEMPLATE" \
  "${PARAMETER_ARGS[@]}"

echo "Validation passed for '$COSMOS_RESOURCE_GROUP' (container-only scope)."
