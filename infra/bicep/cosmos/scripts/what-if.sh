#!/usr/bin/env bash
set -euo pipefail

source "$(dirname "$0")/common.sh"
init_context "${1:-}"
guardrail_assert_incremental_mode

az deployment group what-if \
  --subscription "$AZURE_SUBSCRIPTION_ID" \
  --resource-group "$COSMOS_RESOURCE_GROUP" \
  --template-file "$COSMOS_BICEP_TEMPLATE" \
  "${PARAMETER_ARGS[@]}" \
  --mode "$DEPLOYMENT_MODE"

echo "What-if complete for '$COSMOS_RESOURCE_GROUP' (mode: $DEPLOYMENT_MODE)."
