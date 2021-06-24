CWE-502: Deserialization of Untrusted Data in VeraDemo
======================================================

VeraDemo does not make use of the Forms Encryption but instead implements its own approach.
When a user elects to 'Rmemeber Me' on the login screen, a UserDetails cookie is created that includes serialized information about the user.

You can find this in the ```Verademo.Controllers.AccountController``` class.
This class has a method "PostLogin" which logs a user in and calls ```JsonConvert.SerializeObject```.
This method serializes the ```CustomSerializeModel``` object and puts the result in a ```UserDetails``` cookie.

When the ```GetLogin``` method in the ```Verademo.Controllers.AccountController``` class is executed it calls ```JsonConvert.DeserializeObject<object>``` to deserialize the ```CustomSerializeModel``` object.

This entrusts the browser with supplying serialized .NET code. As the deserialization process necessarily executed code
this is in effect a constrained form of code injection. An attacker can be then abuse this by supplying carefully crafted 
serialized data that, it's deserialization payloads executes shell commands or carries out other malicious actions.

Exploit
-------
We can exploit this like so:

1. Visit the login page and intercept the GET request.
2. Add a cookie called 'UserDetails' if it isn't there already
2. Use the developer tools to set the cookie value and paste the following value: eyIkdHlwZSI6IlZlcmFkZW1vLkNvbW1hbmRzLkFkbWluRXhlY3V0ZUNvbW1hbmQsIGFwcCIsIkFjdGlvbiI6Ii1jIFwidG91Y2ggL3RtcC9oYWNrZWRcIiJ9Cg==
3. Allow the request to go ahead to execute this malicious payload.
4. There will be an error presented but the attack has been successful. Run ls /tmp and observe a file named `hacked`.

Generate the payload
--------------------
1. The payload is simply a Base64-encoded JSON block: ```{"$type":"Verademo.Commands.AdminExecuteCommand, app","Action":"-c \"touch /tmp/hacked\""}```

Mitigate
--------
Performing a validation after a deserialization operation is too late, including if the developer has implemented a cast. If the desired type was passed into JsonConvert.DeserializeObject i.e. `JsonConvert.DeserializeObject<CustomSerializeModel>(...)` instead of `(CustomSerializeModel)JsonConvert.DeserializeObject<object>(...)` then the Newtonsoft JSON library would validate the type and throw an exception before any damage could be done:

```
JsonSerializationException: Type specified in JSON 'Verademo.Commands.AdminExecuteCommand, app, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' is not compatible with 'Verademo.Models.CustomSerializeModel, app, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'. Path '$type', line 1, position 49.
JsonConvert.DeserializeObject<CustomSerializeModel>
This explicit cast enforces the type to be checked prior to potential execution
```

Furthermore the use of `TypeNameHandling.All` should generally be avoided so as to not permit any type being inferred from within the serialized data.
```
new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }
```

Remediate
---------
Do not provide serialized .NET code to untrusted parties.


Resources
---------
* [CWE-502](https://cwe.mitre.org/data/definitions/502.html)
* [pwntester/ysoserial.net](https://github.com/pwntester/ysoserial.net)
* https://speakerdeck.com/pwntester/attacking-net-serialization
* https://www.owasp.org/index.php/Deserialization_Cheat_Sheet#.Net_C.23
* Serialization Binder: https://docs.microsoft.com/en-us/dotnet/framework/wcf/feature-details/controlling-serialization-and-deserialization-with-serializationbinder