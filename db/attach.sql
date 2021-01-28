USE [master]
GO

CREATE DATABASE [Verademo] ON
    (FILENAME = N'/var/opt/mssql/data/Verademo.mdf')
    FOR ATTACH_REBUILD_LOG
GO