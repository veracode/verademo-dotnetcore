CWE-327: Use of a Broken or Risky Cryptographic Algorithm in VeraDemo
========================================================================================================

VeraDemo.NET hashes user password with a known-weak hashing function, MD5.
Using a SQL injection vulnerability, we can extract the hashes from the database.

Exploit
-------
1. Login as a non-admin user (e.g. john)
1. Go to /Search
2. For the search text type in: ' union select username, password, CURRENT_TIMESTAMP from users where username = 'admin'--
3. Observe the addtional result added to the results that includes the username/hashed password.
6. Copy the hash from the 'Blab' field
7. Search Google for the hash and find a site that displays the original value

Mitigate
--------
Use salting when hashing passwords with a longer hash function like SHA-256.

Remediate
---------
Use a strong function for hashing passwords, like bcrypt or PBKDF2.

Resources
---------
* [CWE 327](https://cwe.mitre.org/data/definitions/327.html)
* [Wikipedia: Cryptographic hash function](https://en.wikipedia.org/wiki/Cryptographic_hash_function)
* [Wikipedia: Encryption](https://en.wikipedia.org/wiki/Encryption)
