CWE-200: Information Exposure in VeraDemo.NET
=============================================

VeraDemo has not been properly configured for production use, as such it has
features that are helpful for debugging but also very helpful for attackers.

Exploit
-------
1. Login as a valid user
2. Click on the Search option, or go to /search
3. Enter `o'connor` in the "Search for a blab" box
3. Press Search
4. Observe stack trace with a lot of information, include SQL information.

Others:
* Go to /404 note the IIS and ASP.NET versions
* Go to /tools and check host  "-help", notice the ping command help.


Mitigate
--------
Only show stack trace on during development.

Remediate
---------
Remove stack trace generation, catching and handling exceptions
Apply Custom Errors

* On - If defaultRedirect is specified, they will see that content. Otherwise the default error screen with fewer details.

* Off - Detailed error details will be shown to the user. (the �yellow screen of death screen�)

* RemoteOnly - Default value. Detailed errors only are shown to local users. Remote users receive custom error screens or fewer details.

eg
```
configuration>
  <system.web>
    <customErrors defaultRedirect="YourErrorPage.aspx" 
                  mode="On"> 
      <error statusCode="500"
             redirect="InternalErrorPage.aspx"/>
    </customErrors>
  </system.web>
</configuration>
```