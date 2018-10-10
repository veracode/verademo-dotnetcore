# VeraDemo.Net

## TODO
### Immediate:

* Make it easily deployable into Cloud Services (MS have lots of nice tools to help)
* Test on Greenlight.
* Review the VeraDemo Documentation, test the flaw examples and port what works.
* Move 'correct' SQL over to EF so there are good examples in there too.

### Ongoing:
* More flaws! Review against the VeraDemo docs and achieve Flaw Parity where
language/framework allows.
* Add more CRLF injection for Headers/Logs. Trust Boundary
Violation, CSRF.
* More .NET-specific flaws – review typical ASC findings. Missing Validation Attributes,
Dynamic modification of models.