#!/usr/bin/env bash

set -euo pipefail
source "$(dirname "$0")/common.sh"

compose_cmd down --remove-orphans >/dev/null 2>&1 || true
