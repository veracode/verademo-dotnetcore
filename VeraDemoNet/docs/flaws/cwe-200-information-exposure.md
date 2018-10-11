CWE-200: Information Exposure in VeraDemo.NET
=============================================

VeraDemo has not been properly configured for production use, as such it has
features that are helpful for debugging but also very helpful for attackers.

Exploit
-------
1. Go to /login
2. Fill in for Username: test'
3. Press Login
4. Observe stack trace with a lot of information, include SQL information.

Others:
* Go to /404 note the IIS and ASP.NET versions
* Go to /tools and check host  "-help", notice the ping command help.


Mitigate
--------
Only show stack trace for authorized users.

Remediate
---------
Remove stack trace generation.
Apply Custom Errors, and make them visible to all.