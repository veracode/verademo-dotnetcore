CWE-915: Improperly Controlled Modification of Dynamically-Determined Object Attributes
=======================================================================================

In an MVC model, the data model is often exposed directly to the front-end, even if the front-end doesn't use all the attributes.
In this instance, the 'User' model is exposed. The User model includes a true/false 'IsAdmin' property that wouldn't normally be set in the UI.'


VeraDemo uses untrusted data in concatenation of SQL queries.
This leaves the application vulnerable to malicious users injecting
their own SQL components.

Exploit 1 - Create an Admin (Edit the HTML)
-------------------------------------------
1. Go to `/register`
2. Fill out a new username and hit `Register`
3. On the next screen you'll be asked to fill out details for the user. Edit the HTML using the browser developer tools add add the following into the form: 
```HTML
<input name="IsAdmin" Value="true"/>
```
4. Fill out the rest of the for and hit 'Register'
5. Login as the new user, and go to the Profile. The role should be 'Admin'


Exploit 2 - Create an Admin (Intercept the request)
---------------------------------------------------
1. Go to `/register`
2. Fill out a new username and hit `Register`
3. On the next screen you'll be asked to fill out details for the user. Hit `Intercept` on your intercepting proxy.
4. Fill out the rest of the for and hit `Register`
5. Intercept the response and in the response add: `&IsAdmin=true`
6. Login as the new user, and go to the Profile. The role should be 'Admin'


Mitigate
--------
Validate responses. If a value cannot be set by the user, ensure it is not used.

Remediate
---------
Use a `Bind[Include]` attribute where possible to explicitly define the attributes that the user can set.
Use an appropriate ViewModel approach so that classes do not have unnecessary properties exposed.

Resources
---------
* [CWE 915](https://cwe.mitre.org/data/definitions/915.html)
* [Wikipedia: Mass Assignment](https://en.wikipedia.org/wiki/Mass_assignment_vulnerability)