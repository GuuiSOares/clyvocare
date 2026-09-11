#!/bin/bash
set -euo pipefail
script=/opt/oracle/container-entrypoint.sh
old='    ln -s "${ORACLE_BASE}"/oradata/dbconfig/"${ORACLE_SID}"/orapw"${ORACLE_SID}" "${ORACLE_BASE_CONFIG}"/dbs/orapw"${ORACLE_SID}"'
new='    cp "${ORACLE_BASE}"/oradata/dbconfig/"${ORACLE_SID}"/orapw"${ORACLE_SID}" "${ORACLE_BASE_CONFIG}"/dbs/orapw"${ORACLE_SID}"; chmod 600 "${ORACLE_BASE_CONFIG}"/dbs/orapw"${ORACLE_SID}"'
content=$(cat "$script")
[[ $(grep -Fxc "$old" "$script") = 1 ]] || { echo 'Entrypoint upstream mudou; revisar patch de password file.' >&2; exit 1; }
content=${content/"$old"/"$new"}
printf '%s\n' "$content" > "$script"
bash -n "$script"
