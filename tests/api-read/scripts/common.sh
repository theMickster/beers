#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)"
API_IMAGE_NAME="${API_IMAGE_NAME:-beers-api:quality-gates}"
API_CONTAINER_NAME="${API_CONTAINER_NAME:-beers-api-quality-gates}"
API_PORT="${API_PORT:-8080}"
DOCKER_COMPOSE_FILE="${DOCKER_COMPOSE_FILE:-${ROOT_DIR}/.docker/compose/quality-gates.yml}"
DOCKER_ENV_FILE="${DOCKER_ENV_FILE:-${ROOT_DIR}/.docker/env/.env.api-read}"

load_env_file() {
  local env_file="$1"

  if [[ ! -f "${env_file}" ]]; then
    return 0
  fi

  while IFS= read -r line; do
    line="${line#"${line%%[![:space:]]*}"}"
    line="${line%"${line##*[![:space:]]}"}"

    if [[ -z "${line}" || "${line}" == \#* ]]; then
      continue
    fi

    local key="${line%%=*}"
    local value="${line#*=}"
    key="${key%"${key##*[![:space:]]}"}"
    value="${value#"${value%%[![:space:]]*}"}"

    if [[ -n "${key}" ]]; then
      export "${key}=${value}"
    fi
  done < "${env_file}"
}

load_env_file "${DOCKER_ENV_FILE}"

API_PORT="${API_PORT:-8080}"
BASE_URL="${BASE_URL:-http://localhost:${API_PORT}}"

require_env() {
  local var_name="$1"
  if [[ -z "${!var_name:-}" ]]; then
    echo "Missing required environment variable: ${var_name}" >&2
    exit 1
  fi
}

compose_cmd() {
  local env_args=()
  if [[ -f "${DOCKER_ENV_FILE}" ]]; then
    env_args+=(--env-file "${DOCKER_ENV_FILE}")
  fi

  docker compose \
    -f "${DOCKER_COMPOSE_FILE}" \
    "${env_args[@]}" \
    "$@"
}

wait_for_api() {
  local health_url="${BASE_URL}/swagger/v1/swagger.json"
  local attempts="${API_WAIT_ATTEMPTS:-60}"
  local delay="${API_WAIT_DELAY_SECONDS:-2}"

  echo "Waiting for API readiness at ${health_url}"
  for ((i=1; i<=attempts; i++)); do
    if curl -fsS "${health_url}" >/dev/null 2>&1; then
      echo "API is ready"
      return 0
    fi
    sleep "${delay}"
  done

  echo "API did not become ready in time" >&2
  compose_cmd logs beers-api || true
  exit 1
}
