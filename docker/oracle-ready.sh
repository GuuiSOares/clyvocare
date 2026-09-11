#!/bin/bash
set -euo pipefail
if [ "$(id -u)" = 0 ]; then
  exec setpriv --reuid=oracle --regid=oinstall --init-groups "$0"
fi
sqlplus -L -s / as sysdba <<'SQL'
WHENEVER OSERROR EXIT FAILURE
WHENEVER SQLERROR EXIT FAILURE
SET HEADING OFF FEEDBACK OFF
DECLARE
  n NUMBER;
BEGIN
  SELECT COUNT(*) INTO n FROM v$pdbs WHERE name='XEPDB1' AND open_mode='READ WRITE';
  IF n <> 1 THEN raise_application_error(-20001,'XEPDB1 nao esta READ WRITE'); END IF;
END;
/
ALTER SESSION SET CONTAINER=XEPDB1;
SELECT COUNT(*) FROM clyvocare.TB_CC_PET;
SELECT COUNT(*) FROM clyvocare.TB_CC_LOG_SAUDE;
SELECT 'CLYVOCARE_DATABASE_READY' FROM dual;
EXIT SUCCESS
SQL
