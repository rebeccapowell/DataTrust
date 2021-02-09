using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using Jose;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.JwtSigningCredentials;
using NetDevPack.Security.JwtSigningCredentials.Interfaces;
using Newtonsoft.Json;
using DataTrust.Pod.Api.Infrastructure;
using Swashbuckle.AspNetCore.Annotations;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace DataTrust.Pod.Api.EndPoints.TokenEndPoints
{
    public class Create : BaseAsyncEndpoint<CreateTokenCommand, CreateTokenResult>
    {
        private readonly IJsonWebKeySetService _jwksService;

        private readonly IDataProtector _protector;

        public Create(IJsonWebKeySetService jwksService, IDataProtectionProvider provider)
        {
            _jwksService = jwksService;
            _protector = provider.CreateProtector($"App_{DateTime.UtcNow:yyyy-MM-dd}");
        }

        [HttpPost("api/token")]
        [SwaggerOperation(
            Summary = "Creates a new Token",
            Description = "Creates a new Token",
            OperationId = "token.create",
            Tags = new[] { "TokenEndpoints" })
        ]
        public override async Task<ActionResult<CreateTokenResult>> HandleAsync(
            CreateTokenCommand request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            return Ok(GenerateJwt(request));
        }

        private CreateTokenResult GenerateJwt(CreateTokenCommand request)
        {
            var identityClaims = GetUserClaims(new List<Claim>(), request);
            
            var encodedToken = EncodeToken(identityClaims, request);

            return new CreateTokenResult(request.CorrelationId()) { Token = encodedToken  };
        }

        private ClaimsIdentity GetUserClaims(ICollection<Claim> claims, CreateTokenCommand request)
        {
            // See https://www.iana.org/assignments/jwt/jwt.xhtml
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, request.CorrelationId().ToString("N")));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64));

            var identityClaims = new ClaimsIdentity();
            identityClaims.AddClaims(claims);

            return identityClaims;
        }

        private Dictionary<string, string> GetPrivateClaims(CreateTokenCommand request)
        {
            return new Dictionary<string, string>
            {
                {JwtRegisteredClaimNames.Email, request.Email},
                {"phone_number", request.Telephone},
                {"address", request.Address},
                {"name", request.Fullname}
            };
        }

        private string EncodeToken(ClaimsIdentity identityClaims, CreateTokenCommand request)
        {
            // create a token handler
            var tokenHandler = new JwtSecurityTokenHandler();

            // set the issuer
            var currentIssuer = $"{ControllerContext.HttpContext.Request.Scheme}://{ControllerContext.HttpContext.Request.Host}";

            // get the current SigningCredentials Key
            var signingCredentialsKey = _jwksService.GetCurrent();

            // get and add the claims we are going to protect as an encrypted base64 string
            identityClaims.AddClaim(new Claim("sec", GetProtectedPrivateClaims(request)));

            // return the signed token
            return tokenHandler.CreateEncodedJwt(new SecurityTokenDescriptor
            {
                Issuer = currentIssuer,
                Subject = identityClaims,
                Expires = DateTime.UtcNow.AddDays(30),
                SigningCredentials = signingCredentialsKey
            });
        }

        private string GetProtectedPrivateClaims(CreateTokenCommand request)
        {
            // build the secret claims based on the request
            var secretClaims = GetPrivateClaims(request);

            // serialize the object and get the bytes
            var payLoad = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(secretClaims, Formatting.None));

            // protect that using the Data Protection API using today's key
            var payloadBytes = _protector.Protect(payLoad);

            // return the bytes encoded as a base 64 string
            return Convert.ToBase64String(payloadBytes);
        }

        private static long ToUnixEpochDate(DateTime date)
            => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);

    }
}
