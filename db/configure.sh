#!/usr/bin/env bash

# Start Sql Server
/opt/mssql/bin/sqlservr &
echo "Waiting 30s for Sql Server to start before attaching the Verademo DB..."
sleep 30

# Run the DB attach script
sqlcmd -U SA -P "SuperSecurePassw0rd!" -i /var/opt/mssql/data/attach.sql

# Stop Sql Server
pkill sqlservr