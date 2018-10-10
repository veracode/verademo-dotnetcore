# VeraDemo.Net - Blab-A-Gag Edition

This is a port of the Java VeraDemo. While not all the flaws have been implemented, there are a good selection of SQLi, CRLF etc flaws.


## TODO
### Immediate:

* Make it easily deployable into Cloud Services (MS have lots of nice tools to help)
* Review the VeraDemo Documentation, test the flaw examples and port what works
* Test on Greenlight.
* Move 'correct' SQL over to EF so there are good examples in there too.

### Ongoing:
* Consider whether routed ASPX & Master Pages would reflect the more common approach used by customers 
* More flaws! Review against the VeraDemo docs and achieve Flaw Parity where language/framework allows.
* Add more CRLF injection for Headers/Logs. Trust Boundary Violation, CSRF.
* More .NET-specific flaws – review typical ASC findings. Missing Validation Attributes, Dynamic modification of models.