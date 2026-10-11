#!/usr/bin/env bash
# Verifica se a API de um ambiente está no ar.
# Uso: ./scripts/smoke-test.sh [url]   (padrão: http://localhost:8080)
set -euo pipefail
URL="${1:-http://localhost:8080}"
echo "Testando ${URL}/health ..."
curl -fsS --retry 30 --retry-delay 5 --retry-all-errors "${URL}/health"
echo
echo "OK: API respondendo em ${URL}"
