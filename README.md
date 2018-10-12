# VeraDemo.NET - Blab-a-Gag

## About

Blab-a-Gag is a fairly simple forum type application which allows:
 - users to post a one-liner joke
 - users to follow the jokes of other users or not (listen or ignore)
 - users to comment on other users messages (heckle)
 
### URLs

`/reset` will reset the data in the database with a load of:
 - users
 - jokes
 - heckles
  
`/feed` shows the jokes/heckles that are relevant to the current user.

`/blabbers` shows a list of all other users and allows the current user to listen or ignore.

`/profile` allows the current user to modify their profile.

`/login` allows you to log in to your account

`/register` allows you to create a new user account

`/tools` shows a tools page that shows a fortune or lets you ping a host.
   
## Configure

Database credentials are held in web.config
Log4Net information is helped in log4net.config

### Database

A blank database is provided in App_Data\VeraDemoNet.mdf - the application will connect to this by default.
If you want to change it, the connection string is in web.config as BlabberDB
 
## Run

Visual Studio 2017 is required to build the application. Publishing generates the appropriate files to deploy.

Alternatively, run from inside Visual Studio.

Open `/reset` in your browser and follow the instructions to prep the database

Login with your username/password as defined in `ResetController.cs :: _veraUsers

## AWS/Azure Deployment

TODO

## Exploitation Demos

See the `docs` folder


# TODO

This is a port of the Java VeraDemo. While not all the flaws have been implemented, there are a good selection of SQLi, CRLF etc flaws.

## Immediate:

* Make it easily deployable into Cloud Services (MS have lots of nice tools to help)
* Review the VeraDemo Documentation, test the flaw examples and port what works
* Test on Greenlight.
* Move 'correct' SQL over to EF so there are good examples in there too.

## Ongoing:
* Consider whether routed ASPX & Master Pages would reflect the more common approach used by customers 
* More flaws! Review against the VeraDemo docs and achieve Flaw Parity where language/framework allows.
* Add more CRLF injection for Headers/Logs. Trust Boundary Violation, CSRF.
* More .NET-specific flaws – review typical ASC findings. Missing Validation Attributes, Dynamic modification of models.