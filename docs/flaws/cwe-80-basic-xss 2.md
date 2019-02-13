CWE-80: Improper Neutralization of Script-Related HTML Tags in a Web Page (Basic XSS) in VeraDemo
=================================================================================================

VeraDemo mixes untrusted data with HTML, leading to the possibility of an attacker
injecting their own HTML that may do anything with the page.

Exploit
-------
1. Visit the login screen.
2. Login with one of the fields missing
3. The page redirects to itself, but adds an extra 'Message' parameter dictating what will appear on the screen as an error.
4. Adjust the Message parameter to read the following, and visit the url:

	`Login?Message=<script>alert('Hacked by a Hacker');</script>`
5. Observe an alert box `Hacked by a Hacker`.

Mitigate
--------
Avoid rendering HTML from properties. In the event of an error, dictate the formatting as part of the application so that you can encode the message itself.

For ASP.NET a similar approach can be used with the following if the application fails model validation

`
ModelState.AddModelError("", "Something Wrong : UserName or Password invalid ^_^ ");  
`

and use the following in the page to render it, where you can specify your own CSS:

`
@Html.ValidationSummary(true, "", new {@class = "text-danger"})
`

Remediate
---------
Encode data using [OWASP Java Encoder Project](https://www.owasp.org/index.php/OWASP_Java_Encoder_Project).

Resources
---------
* [CWE 80](https://cwe.mitre.org/data/definitions/80.html)
* [OWASP: OWASP Java Encoder Project](https://www.owasp.org/index.php/OWASP_Java_Encoder_Project)