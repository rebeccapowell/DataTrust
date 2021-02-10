![Logo](https://repository-images.githubusercontent.com/337559635/9498fd80-6b42-11eb-8b67-b97600166167)
# DataTrust
Experiments with third party data trust models

# DataTrust.Pod.Api
This is a demonstration of how we can leverage existing security mechanisms built into OAuth using JWT (Json Web Tokens) and JWK (Json Web Keys), with a JWKS (Json Web Token Key Set).

JWT is has a well-defined schema that has been ratified by the W3C, and it allows third parties to provide user data from an authentication server, to an interested third party. It is in essence, authentication as a service. 

Every JWT should be signed, and the most common use case is to use a shared symmetric key to sign the JWT before returning it to the third party who, also with that symmetric key can validate the signature, ensuring that the signature is valid.

These can also be encrypted, again usually with a shared symmetric key. This allows the token to be shared, but only a party with the key can read it, otherwise the token can easily be read using a service such as http://jwt.io.

The proposal here, is to take this one step further. Without extending the JWT definition, we can use a "claim" as a container for an encrypted packet, that is not accessible to the third party, but using public key cryptography can secure that data, and publish the public key using JWKS, so that third parties can check the signature, but decrypt its contents.

This concept has some interesting applications. There are various scenarios, where you wish to enter a binding contract, and offer data, but not give access to it until a future point in time.

Examples of this could be job applications, pandemic personal track and trace location data, or consumer data as part of an e-commerce transaction.

In essence, a user can give the data to a trusted party and offer that data to a party they don't fully trust, only giving access to that data when absolutely required.

# Worked example:
## Create a token:

```
POST /api/token HTTP/1.1
Host: localhost:44396
Content-Type: application/json
Content-Length: 143

{
    "FullName": "Rebecca Powell",
    "Address": "My address, Bremen",
    "Telephone": "0176444444",
    "Email": "email@address.com"
}
```

Gives the response:

```
{
    "token": "eyJhbGciOiJSUzUxMiIsImtpZCI6ImZCZFNWVWU2X2RsX0NGUlpocklpTHciLCJ0eXAiOiJKV1QifQ.eyJzdWIiOiJkYmYxNmI1NzYzYmQ0NGM4YjZmYmJlNmRjMzQwNWEwMSIsImp0aSI6ImEwODAzODM1LTQ3YzYtNGI4My1iZWI4LWQ3NjRiOTk0ZTE3OSIsIm5iZiI6MTYxMjkxMzA5MCwiaWF0IjoxNjEyOTEzMDkwLCJzZWMiOiJDZkRKOEc0L2dYNnlrTTlPcnBhTW9teFlFMDZoTkcyUjErM2R0dnZESmhlbVp1aElNaEVydVNlSndNTWpQKy9yNitHRkpnRFc1S0Njc0FZSEVjaXh6Z2psQjhPZUJHanlKYitzMC9tc1RZdWx2aHIxc3hHL1NhTmdLQ1ZSRU1KOGNpaFZhd3k0UFJxMDQ1amF4OWZDbTJyWW50ZFVsL0NqOWtRUDlyc1VRVGRYeUFDSWpBSkN5TW56NCthMml2ZFo5RlBTMU9Ub0kwU2NMOHRpcUJuMkhKd09tNFNBNzZIZmVNSEozWVM5SDROV2JwL0pwdThqRUlPTmZHblVQWFN6cU5LL001VStTS1lsNTNYMGV5V1JPKzNpZXVFPSIsImV4cCI6MTYxNTUwNTA5MCwiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6NDQzOTYifQ.BjO7PW6wwfDnGbAE22mM_I58iTIXyCpLpqe1o8SiEDMZs59OgTj7fRVEsFAYEpNCIlyZ5LD64AAy6K1mSAP6gGR2qDY9Y64-H5e7US50aUz6XsF2jA3uh5PpKw2l1j9OWmya6XcbjKaY5mlK1ir9_kag96zZKVCm_c4N7T44E5E0uAnyD4bCZexzHgLsetGUCLPMOw94WLwfYQqiT-u1SKaBGS2b-V3kAkywUR0agapXWRMvM7uD1-dFWiM0kCLHInZmVliKXCp9-u5Azrqbs216UoqsqwR4fbTvJdbxhzIJP5O8Kh7tAhrzgwRiS0wLk4K301JoGqZnw4hvyEKjww"
}
```

Using JWT.io, we can see that this token comprises of these constituent parts:

A header:
```
{
  "alg": "RS512",
  "kid": "fBdSVUe6_dl_CFRZhrIiLw",
  "typ": "JWT"
}
```

A payload:
```
{
  "sub": "dbf16b5763bd44c8b6fbbe6dc3405a01",
  "jti": "a0803835-47c6-4b83-beb8-d764b994e179",
  "nbf": 1612913090,
  "iat": 1612913090,
  "sec": "CfDJ8G4/gX6ykM9OrpaMomxYE06hNG2R1+3dtvvDJhemZuhIMhEruSeJwMMjP+/r6+GFJgDW5KCcsAYHEcixzgjlB8OeBGjyJb+s0/msTYulvhr1sxG/SaNgKCVREMJ8cihVawy4PRq045jax9fCm2rYntdUl/Cj9kQP9rsUQTdXyACIjAJCyMnz4+a2ivdZ9FPS1OToI0ScL8tiqBn2HJwOm4SA76HfeMHJ3YS9H4NWbp/Jpu8jEIONfGnUPXSzqNK/M5U+SKYl53X0eyWRO+3ieuE=",
  "exp": 1615505090,
  "iss": "https://localhost:44396"
}
```

And finally, a signature, that is created using RSASHA512.

## Token Validation

In order to validate this signature, we need the public key. This API offers a key set endpoint. It can be called using a simple GET request:

```
GET /jwks HTTP/1.1
Host: localhost:44396
Content-Type: application/json
```

Which gives the response:
```
{"keys":[{"kty":"RSA","use":"sig","kid":"fBdSVUe6_dl_CFRZhrIiLw","n":"n9qOibxaZ-V9JEZRFocefJa3n9xUBIn8LJ66Jjwv8fSvOAALIctTVYChetDi6LMbgEHtsizrwsjgO78S__xsyweNnCqZxNLuyDHWhy2oKRuYrIPqzGCMFyocVaJ38GOwAWeAjXpteKZG4ZMr0J59AqBSATJYhqt6SPgnVNilpw720wRT-G7gAStDYKWqcFrCQI6J3_KlbaPgO5zIUz4NKeGkkNTL0E77ceoNxw2vSPQsPWumuH7Vne0LwtiPKUauhUQv_yW9KBytgeTQAjVwVPFae2J3IfyhzXkG_6Zlz_vIQwZnGkvL-60igQXUDDGW3KRgDNo_fsmJre13cIujtQ","e":"AQAB"}]}
```

We can now use that public key information to validate the JWT:

```
var handler = new JwtSecurityTokenHandler();
var jsonToken = handler.ReadToken(result.Token) as JwtSecurityToken;
var kid = jsonToken.Header.Kid;

var keys = jwksResult.GetSigningKeys();
var key = keys.FirstOrDefault(x => x.KeyId == kid);

var validations = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = key,
    ValidateIssuer = false,
    ValidateAudience = false
};
var claimsPrincipal = handler.ValidateToken(result.Token, validations, out var tokenSecure);

Assert.Equal(7, claimsPrincipal.Claims.Count());
```

For further information, you can run the associated end to end tests in the test project.

#Decoding the token

The API also offers a decode token call, that takes the token you were given above, and decodes it back for you:

```
PUT /api/token HTTP/1.1
Host: localhost:44396
Content-Type: application/json
Content-Length: 1061

{
    "token": "eyJhbGciOiJSUzUxMiIsImtpZCI6ImZCZFNWVWU2X2RsX0NGUlpocklpTHciLCJ0eXAiOiJKV1QifQ.eyJzdWIiOiJkYmYxNmI1NzYzYmQ0NGM4YjZmYmJlNmRjMzQwNWEwMSIsImp0aSI6ImEwODAzODM1LTQ3YzYtNGI4My1iZWI4LWQ3NjRiOTk0ZTE3OSIsIm5iZiI6MTYxMjkxMzA5MCwiaWF0IjoxNjEyOTEzMDkwLCJzZWMiOiJDZkRKOEc0L2dYNnlrTTlPcnBhTW9teFlFMDZoTkcyUjErM2R0dnZESmhlbVp1aElNaEVydVNlSndNTWpQKy9yNitHRkpnRFc1S0Njc0FZSEVjaXh6Z2psQjhPZUJHanlKYitzMC9tc1RZdWx2aHIxc3hHL1NhTmdLQ1ZSRU1KOGNpaFZhd3k0UFJxMDQ1amF4OWZDbTJyWW50ZFVsL0NqOWtRUDlyc1VRVGRYeUFDSWpBSkN5TW56NCthMml2ZFo5RlBTMU9Ub0kwU2NMOHRpcUJuMkhKd09tNFNBNzZIZmVNSEozWVM5SDROV2JwL0pwdThqRUlPTmZHblVQWFN6cU5LL001VStTS1lsNTNYMGV5V1JPKzNpZXVFPSIsImV4cCI6MTYxNTUwNTA5MCwiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6NDQzOTYifQ.BjO7PW6wwfDnGbAE22mM_I58iTIXyCpLpqe1o8SiEDMZs59OgTj7fRVEsFAYEpNCIlyZ5LD64AAy6K1mSAP6gGR2qDY9Y64-H5e7US50aUz6XsF2jA3uh5PpKw2l1j9OWmya6XcbjKaY5mlK1ir9_kag96zZKVCm_c4N7T44E5E0uAnyD4bCZexzHgLsetGUCLPMOw94WLwfYQqiT-u1SKaBGS2b-V3kAkywUR0agapXWRMvM7uD1-dFWiM0kCLHInZmVliKXCp9-u5Azrqbs216UoqsqwR4fbTvJdbxhzIJP5O8Kh7tAhrzgwRiS0wLk4K301JoGqZnw4hvyEKjww"
}
```

Gives the response:

```
{
    "claims": {
        "sub": "dbf16b5763bd44c8b6fbbe6dc3405a01",
        "jti": "a0803835-47c6-4b83-beb8-d764b994e179",
        "nbf": "1612913090",
        "iat": "1612913090",
        "exp": "1615505090",
        "iss": "https://localhost:44396",
        "email": "email@address.com",
        "phone_number": "0176444444",
        "address": "My address, Bremen",
        "name": "Rebecca Powell"
    }
}
```

Final note, this is using the new Microsoft Data Protection API. I've set it up to rotate keys daily and each key expires after 30 days. 

Considering something like a pandemic response, this data does not need to be held perpetually, and in fact only needs to be held for around 14 days, so this kind of model would be extremely useful to protect the publics data, whilst guaranteeing that the data is safe with a third party.

