#!/bin/bash
set -euo pipefail
target="${ORACLE_BASE}/oradata/dbconfig/${ORACLE_SID}/orapw${ORACLE_SID}"
cp "${ORACLE_BASE_CONFIG}/dbs/orapw${ORACLE_SID}" "${target}.pending"
mv -f "${target}.pending" "$target"
