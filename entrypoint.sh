#!/usr/bin/env bash

/opt/mssql/bin/sqlservr > /dev/null 2>&1 & disown
dotnet run