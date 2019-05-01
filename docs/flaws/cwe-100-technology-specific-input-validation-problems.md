CWE-100: Technology-Specific Input Validation Problems
======================================================

Verademo.NET takes values from the user without applying validation. .NET provides validation attributes that can assist in the per-property validation defintion.


Exploit
-------
1. Login
2. Go to `/profile` and change the username to a single character

Mitigation
----------
* Validate that the username is appropriate in-code based on security policy (eg alphanumeric with underscores)

Remediation
-----------
* Apply an appropriate ValidationAttribute on the incoming properties based on security policy.

Resources
---------
* [CWE-100](https://cwe.mitre.org/data/definitions/100.html)
* https://msdn.microsoft.com/en-us/library/ee256141(v=vs.100).aspx
* http://www.asp.net/web-api/overview/formats-and-model-binding/model-validation-in-aspnet-web-api