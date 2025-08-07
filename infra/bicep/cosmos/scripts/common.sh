#!/usr/bin/env bash
set -euo pipefail

readonly DEPLOYMENT_MODE="Incremental"
readonly SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
readonly REPO_ROOT="$(git -C "$SCRIPT_DIR" rev-parse --show-toplevel 2>/dev/null || pwd)"
readonly DEFAULT_TEMPLATE_RELATIVE_PATH="infra/bicep/cosmos/main.bicep"
readonly DEFAULT_PARAMS_RELATIVE_PATH="infra/bicep/cosmos/parameters/current-env.json"

usage() {
  echo "Usage: bash infra/bicep/cosmos/scripts/<validate|what-if|deploy>.sh <dev|qa>"
}

guardrail_assert_incremental_mode() {
  [[ "$DEPLOYMENT_MODE" == "Incremental" ]] || die "Guardrail violation: deployment mode must be Incremental (got '$DEPLOYMENT_MODE')."
}

die() {
  echo "ERROR: $*" >&2
  exit 1
}

require_command() {
  local name="$1"
  command -v "$name" >/dev/null 2>&1 || die "Required command not found: $name"
}

require_env() {
  local name="$1"
  if [[ -z "${!name:-}" ]]; then
    die "Missing required environment variable: $name"
  fi
}

to_absolute_path() {
  local input_path="$1"
  if [[ "$input_path" = /* ]]; then
    python3 -c 'import os,sys; print(os.path.realpath(sys.argv[1]))' "$input_path"
    return
  fi

  python3 -c 'import os,sys; print(os.path.realpath(sys.argv[1]))' "$REPO_ROOT/$input_path"
}

guardrail_assert_template_is_valid_for_group_deployment() {
  [[ "$COSMOS_BICEP_TEMPLATE" = *.bicep ]] || die "Template must be a .bicep file (got '$COSMOS_BICEP_TEMPLATE')."
  [[ -f "$COSMOS_BICEP_TEMPLATE" ]] || die "Template not found: $COSMOS_BICEP_TEMPLATE"

  local template_scope
  template_scope="$(awk -F"'" '/^[[:space:]]*targetScope[[:space:]]*=/ { print $2; exit }' "$COSMOS_BICEP_TEMPLATE")"
  [[ "$template_scope" == "resourceGroup" ]] || die "Template targetScope must be 'resourceGroup' for az deployment group commands (got '${template_scope:-unset}')."
}

guardrail_assert_container_only_template() {
  local template_dir
  template_dir="$(dirname "$COSMOS_BICEP_TEMPLATE")"
  local violations=""
  local file=""

  while IFS= read -r file; do
    local file_violations
    file_violations="$(awk '
      /resource[[:space:]].*Microsoft\.DocumentDB\/databaseAccounts(@|\/sqlDatabases@)/ {
        if ($0 !~ / existing[[:space:]]*=/) {
          print FNR ":" $0
          found = 1
        }
      }
      END {
        if (!found) {
          exit 1
        }
      }
    ' "$file" || true)"

    if [[ -n "$file_violations" ]]; then
      violations+=$'\n'"$file"
      violations+=$'\n'"$file_violations"
    fi
  done < <(find "$template_dir" -type f -name '*.bicep' -print)

  if [[ -n "$violations" ]]; then
    die "Guardrail violation: template set includes Cosmos account/database mutation declarations. Only existing account/database references are allowed.$violations"
  fi
}

template_parameter_names() {
  awk '/^[[:space:]]*param[[:space:]]+[A-Za-z_][A-Za-z0-9_]*[[:space:]]*/ { print $2 }' "$COSMOS_BICEP_TEMPLATE"
}

validate_json_parameter_file() {
  local file_path="$1"
  local template_params
  local json_params
  local missing=""
  local unknown=""
  local param=""

  template_params="$(template_parameter_names)"
  json_params="$(python3 -c 'import json,sys; print("\n".join(json.load(open(sys.argv[1], encoding="utf-8")).get("parameters", {}).keys()))' "$file_path")" \
    || die "Failed to parse JSON parameter file: $file_path"

  while IFS= read -r param; do
    [[ -z "$param" ]] && continue
    if ! grep -qx "$param" <<<"$template_params"; then
      unknown+="${unknown:+, }$param"
    fi
  done <<<"$json_params"

  local required_params=("cosmosAccountName" "cosmosDatabaseName" "containers")
  for param in "${required_params[@]}"; do
    if ! grep -qx "$param" <<<"$json_params"; then
      missing+="${missing:+, }$param"
    fi
  done

  if [[ -n "$missing" ]]; then
    die "Parameter file '$file_path' is missing required parameters: $missing"
  fi

  if [[ -n "$unknown" ]]; then
    die "Parameter file '$file_path' includes parameters not defined by template '$COSMOS_BICEP_TEMPLATE': $unknown"
  fi
}

validate_bicepparam_file() {
  local file_path="$1"
  local using_target
  using_target="$(awk -F"'" '/^[[:space:]]*using[[:space:]]+/ { print $2; exit }' "$file_path")"

  [[ -n "$using_target" ]] || die "Parameter file '$file_path' must declare a 'using' template."

  local resolved_using
  resolved_using="$(to_absolute_path "$(dirname "$file_path")/$using_target")"

  if [[ "$resolved_using" != "$COSMOS_BICEP_TEMPLATE" ]]; then
    die "Parameter file '$file_path' targets '$resolved_using', but script template is '$COSMOS_BICEP_TEMPLATE'."
  fi
}

guardrail_assert_parameter_file_is_valid() {
  [[ -n "${COSMOS_PARAMS_FILE:-}" ]] || return

  [[ -f "$COSMOS_PARAMS_FILE" ]] || die "Parameter file not found: $COSMOS_PARAMS_FILE"

  case "$COSMOS_PARAMS_FILE" in
    *.json)
      validate_json_parameter_file "$COSMOS_PARAMS_FILE"
      ;;
    *.bicepparam)
      validate_bicepparam_file "$COSMOS_PARAMS_FILE"
      ;;
    *)
      die "Unsupported parameter file extension for '$COSMOS_PARAMS_FILE'. Use .json or .bicepparam."
      ;;
  esac
}

resolve_default_params_file() {
  if [[ -n "${COSMOS_PARAMS_FILE:-}" ]]; then
    return
  fi

  local default_params_abs
  default_params_abs="$(to_absolute_path "$DEFAULT_PARAMS_RELATIVE_PATH")"
  if [[ -f "$default_params_abs" ]]; then
    COSMOS_PARAMS_FILE="$default_params_abs"
  fi
}

build_parameter_args() {
  PARAMETER_ARGS=()
  if [[ -n "${COSMOS_PARAMS_FILE:-}" ]]; then
    case "$COSMOS_PARAMS_FILE" in
      *.bicepparam)
        PARAMETER_ARGS+=(--parameters "$COSMOS_PARAMS_FILE")
        ;;
      *)
        PARAMETER_ARGS+=(--parameters @"$COSMOS_PARAMS_FILE")
        ;;
    esac
    return
  fi

  local account_name="${COSMOS_ACCOUNT_NAME:-}"
  if [[ -z "$account_name" ]]; then
    account_name="$COSMOS_ACCOUNT_NAME_ENV"
  fi

  [[ -n "$account_name" ]] || die "No parameter file provided. Set COSMOS_PARAMS_FILE (recommended) or COSMOS_ACCOUNT_NAME / COSMOS_ACCOUNT_NAME_<ENV>."
  require_env "COSMOS_DATABASE_NAME"
  [[ -n "${COSMOS_CONTAINERS_JSON:-}" ]] || die "No parameter file provided. Set COSMOS_CONTAINERS_JSON to a JSON array of containers."

  PARAMETER_ARGS+=(
    --parameters
    "cosmosAccountName=$account_name"
    "cosmosDatabaseName=$COSMOS_DATABASE_NAME"
    "containers=$COSMOS_CONTAINERS_JSON"
  )
}

init_context() {
  local target_env="${1:-}"
  if [[ -z "$target_env" ]]; then
    usage
    exit 1
  fi

  case "$target_env" in
    dev)
      require_env "COSMOS_RESOURCE_GROUP_DEV"
      COSMOS_RESOURCE_GROUP="$COSMOS_RESOURCE_GROUP_DEV"
      COSMOS_ACCOUNT_NAME_ENV="${COSMOS_ACCOUNT_NAME_DEV:-}"
      COSMOS_PARAMS_FILE="${COSMOS_PARAMS_DEV:-${COSMOS_PARAMS_FILE:-}}"
      ;;
    qa)
      require_env "COSMOS_RESOURCE_GROUP_QA"
      COSMOS_RESOURCE_GROUP="$COSMOS_RESOURCE_GROUP_QA"
      COSMOS_ACCOUNT_NAME_ENV="${COSMOS_ACCOUNT_NAME_QA:-}"
      COSMOS_PARAMS_FILE="${COSMOS_PARAMS_QA:-${COSMOS_PARAMS_FILE:-}}"
      ;;
    *)
      usage
      die "Invalid environment: $target_env (expected dev or qa)"
      ;;
  esac

  [[ -n "$COSMOS_RESOURCE_GROUP" ]] || die "Resolved resource group is empty for environment '$target_env'."

  require_command "az"
  require_command "python3"
  require_env "AZURE_SUBSCRIPTION_ID"
  az account show >/dev/null 2>&1 || die "Azure CLI is not logged in. Run 'az login' and retry."
  local active_subscription_id
  active_subscription_id="$(az account show --query id -o tsv 2>/dev/null || true)"
  if [[ -n "$active_subscription_id" && "$active_subscription_id" != "$AZURE_SUBSCRIPTION_ID" ]]; then
    die "Active Azure CLI subscription ('$active_subscription_id') does not match AZURE_SUBSCRIPTION_ID ('$AZURE_SUBSCRIPTION_ID'). Run: az account set --subscription \"$AZURE_SUBSCRIPTION_ID\""
  fi

  COSMOS_BICEP_TEMPLATE="$(to_absolute_path "${COSMOS_BICEP_TEMPLATE:-$DEFAULT_TEMPLATE_RELATIVE_PATH}")"
  guardrail_assert_template_is_valid_for_group_deployment
  guardrail_assert_container_only_template
  resolve_default_params_file
  if [[ -n "${COSMOS_PARAMS_FILE:-}" ]]; then
    COSMOS_PARAMS_FILE="$(to_absolute_path "$COSMOS_PARAMS_FILE")"
  fi
  guardrail_assert_parameter_file_is_valid
  build_parameter_args
}
