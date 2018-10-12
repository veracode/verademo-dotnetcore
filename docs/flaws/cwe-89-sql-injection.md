CWE-89: Improper Neutralization of Special Elements used in an SQL Command ('SQL Injection') in VeraDemo
========================================================================================================

VeraDemo uses untrusted data in concatenation of SQL queries.
This leaves the application vulnerable to malicious users injecting
their own SQL components.

Exploit 1 - 'blind' SQLi
------------------------
1. Go to /login
2. For Username fill in: admin' or 1 -- 
3. Observe that you can bypass authentication and log in as the admin user.


Exploit 2 - classic SQLi
------------------------
1. Login and go to /Search
2. For the search text type in: ' union select username, password, CURRENT_TIMESTAMP from users where username = 'admin'--
3. Observe the addtional result added to the results that includes the username/hashed password.


Mitigate
--------
Use a whitelist to ensure data (like a username) only contains alphanumeric characters.

Remediate
---------
Query the data using prepared statements.

Resources
---------
* [CWE 89](https://cwe.mitre.org/data/definitions/89.html)
* [Wikipedia: SQL injection](https://en.wikipedia.org/wiki/SQL_injection)