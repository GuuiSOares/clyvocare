#!/bin/bash
mkdir -p /opt/oracle/product/21c/dbhomeXE/network/log
rm -f /opt/oracle/product/21c/dbhomeXE/network/log/listener.log
chmod 777 /opt/oracle/product/21c/dbhomeXE/network/log 2>/dev/null || true
chmod 777 /opt/oracle/oradata 2>/dev/null || true
exec /opt/oracle/container-entrypoint.sh "$@"
