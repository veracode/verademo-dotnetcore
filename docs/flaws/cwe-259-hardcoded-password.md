CWE-259: Harcoded Password
======================================================

Verademo.NET harcodes credentials for the database in the BlabberDb class.
You can find this class at DataAccess\BlabberDB.cs .

Remediation
-----------
* Move connection string to web.config file and fetch the connectionString by name.

Resources
---------
* [CWE-259](https://cwe.mitre.org/data/definitions/259.html)
* https://docs.microsoft.com/en-us/ef/ef6/fundamentals/configuring/connection-strings