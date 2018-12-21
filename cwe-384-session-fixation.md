CWE-384: Session Fixation in VeraDemo
=====================================

A webserver has no means of identifying one request from another.
In order to do so it has the concept of a 'session' identified by
as session cookie. Any request with that cookie is identified as
belonging to that session.
Typically sessions hold authentication information making them
very valuable for attackers.
Attackers may try to steal session identifiers.
Or they may attempt to simply provide their own session identifier
and let the victim log into that.
This demo does just that with an XSS flaw and a weakened session cookie. 

Exploit
-------
** Warning, use FireFox. Do not use Chrome or another browser (for victim) with an XSS Auditor in this demo! **
1. Open a normal window (attacker) and a 'Private Window' (victim).
2. As an attacker visit the website, you'll be redirected to login page. 
   Open up the Dev tools. In the Console tab, enter "document.cookie" (no quotes).
   You'll see that the value is empty as the cookie is protected using HttpOnly attribute.
   2.1 Let's try different approach - in development tools there's "Application" (in Chrome) or Storage (in Firefox) section,
   on the left Cookies entry will appear, click on the name of the domain that you're running this website on - click on it
   2.2 Copy the identifier and value into notepad and prepare following JS snippet:
   document.cookie="ASP.NET_SessionId=<REPLACE_ME>", run in in the console. While you should try with real identifier instead of <REPLACE_ME> you can try any value. You'll see that nothing changed, browser doesn't allow us 
   to downgrade security of the cookie
   2.3 Luckily for the attacker there's page that doesn't establish authentication cookie and has cross site scripting vulnerability so if user does not have
   session cookie present we'll be able in inject our session identifier! Let's try that 

3. Craft the following link, where <REPLACE_ME> (including angle brackets) is replaced by the ASP.NET_SessionId value copied in the previous step:
   <DOMAIN_HERE>:<PORT_HERE>/passwordhint?username=<script>document.cookie="ASP.NET_SessionId=<REPLACE_ME>";</script>
4. Paste the link in the victim window.
5. Let the victim log in.
6. Open <DOMAIN_HERE>:<PORT_HERE>/Profile URL in the attacker window.
7. Note you are logged in as the victim.

Mitigate
--------
Make sure that every page must have session identifier set prior to sending data back to the user so the 
browser doesn't allow degradation of security control. Make sure that all session-related cookies have Secure and HttpOnly flag.

Remediate
---------
Change the session id after login.