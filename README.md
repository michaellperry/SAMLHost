# SAMLHost

Based on the example from [nucleoid/SamlSSO](https://github.com/nucleoid/SamlSSO).

[Set up your own custom SAML application](https://apps.google.com/supportwidget/articlehome?hl=en&article_url=https%3A%2F%2Fsupport.google.com%2Fa%2Fanswer%2F6087519%3Fhl%3Den&product_context=6087519&product_name=UnuFlow&trigger_context=a)

- Configure a host name containing a dot (Google does not allow localhost)
	- Edit "C:\Windows\System32\drivers\etc\hosts" as administrator
	- Add the line `127.0.0.1	sso.localhost`
- Add Custom SAML App [Google Admin: Apps](https://admin.google.com/ac/apps/unified)
	- App name: SAML Service Provider
	- Description: Host web application that simulates a SAML service provider
	- ACS URL: https://sso.localhost:7006/Consume
	- Entity ID: https://sso.localhost:7006
	- Service Status: On for Everyone
- Record the following
	- SSO URL: 
	- Entity ID: 
	- Certificate (download as .pem): 

## User Secrets

Right-click on the SAMLHost project in Visual Studio and "Manage User Secrets". This opens
a `secret.json` file. Copy the "SAMLIdentityProvider" section from the `appsettings.json`
file and paste it into the `secret.json` file. Replace the values with the values from the
Google Admin console.

## Troubleshooting
To troubleshoot SAML, you can use the [SAML-tracer](https://chrome.google.com/webstore/detail/saml-tracer/mpdajninpobndbfcldcmbpnnbhibjmch?hl=en) Chrome extension.

- Invalid ACS URL
	- Include a dot: https://sso.localhost:7006/Consume
	- Define an alias in hosts
- **Error: app_not_configured_for_user**
	- [Verify the value in the saml:Issuer tag](https://apps.google.com/supportwidget/articlehome?hl=en&article_url=https%3A%2F%2Fsupport.google.com%2Fa%2Fanswer%2F6301076%3Fhl%3Den&product_context=6301076&product_name=UnuFlow&trigger_context=a)
	- Before the login page: bad Issuer
	- After the login page:
		- Issuer is https://sso.localhost:7006
		- Should match the Entity ID
		- Change the Entity ID
	- Go incognito
		- If you are prompted to log in, the app configuration is correct, but the app is not activated for your Google user. Turn the service status on for everyone.
- Null reference exception
```
System.NullReferenceException
  HResult=0x80004003
  Message=Object reference not set to an instance of an object.
  Source=System.Security.Cryptography.Xml
  StackTrace:
   at System.Security.Cryptography.Xml.SignedXml.IsKeyTheCorrectAlgorithm(AsymmetricAlgorithm key, Type expectedType)
   at System.Security.Cryptography.Xml.SignedXml.CheckSignedInfo(AsymmetricAlgorithm key)
   at System.Security.Cryptography.Xml.SignedXml.CheckSignature(AsymmetricAlgorithm key)
   at System.Security.Cryptography.Xml.SignedXml.CheckSignature(X509Certificate2 certificate, Boolean verifySignatureOnly)
   at SAMLHost.Services.SamlService.CheckSignature(SignedXml signedXml, X509Certificate2 cert) in SAMLHost\Services\SamlService.cs:line 73
```
    - CryptoConfig no longer requires a call to AddAlgorithm. If this call is made, then CheckSignature will throw a null reference exception.