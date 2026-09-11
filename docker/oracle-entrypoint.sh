#!/bin/bash
set -euo pipefail

echo "CONTAINER: clyvocare entrypoint uid=$(id -u) gid=$(id -g)"

mkdir -p /opt/oracle/product/21c/dbhomeXE/network/log
rm -f /opt/oracle/product/21c/dbhomeXE/network/log/listener.log
chmod 777 /opt/oracle/product/21c/dbhomeXE/network/log 2>/dev/null || true

mkdir -p /opt/oracle/oradata
chmod 777 /opt/oracle/oradata 2>/dev/null || true

if [ "$(id -u)" = "0" ]; then
  echo "CONTAINER: dropping to user oracle for official entrypoint"
  exec setpriv --reuid=oracle --regid=oinstall --init-groups /oracle-entrypoint.sh "$@"
fi

dump_failure() {
  local status=$?
  if [ "$status" -ne 0 ]; then
    echo "CONTAINER: Oracle falhou, exit=$status; alert.log:"
    for alert in /opt/oracle/diag/rdbms/*/*/trace/alert_*.log; do
      [ ! -f "$alert" ] || tail -n 160 "$alert" || true
    done
  fi
}
trap dump_failure EXIT
/opt/oracle/container-entrypoint.sh "$@" &
oracle_pid=$!
trap 'kill -TERM "$oracle_pid" 2>/dev/null || true; wait "$oracle_pid"; exit $?' TERM INT
wait "$oracle_pid"
