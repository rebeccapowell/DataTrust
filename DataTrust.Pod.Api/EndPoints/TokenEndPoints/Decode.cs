// // -----------------------------------------------------------------------
// // <copyright file="Decode.cs" company="PanaTrace" year="2020">
// //      All rights are reserved. Reproduction or transmission in whole or
// //      in part, in any form or by any means, electronic, mechanical or
// //      otherwise, is prohibited without the prior written consent of the
// //      copyright owner.
// // </copyright>
// // <summary>
// //      Definition of the Decode.cs class.
// // </summary>
// // -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.JwtSigningCredentials.Interfaces;
using Newtonsoft.Json;
using DataTrust.Pod.Api.Infrastructure;
using Swashbuckle.AspNetCore.Annotations;

namespace DataTrust.Pod.Api.EndPoints.TokenEndPoints
{
    public class Decode : BaseAsyncEndpoint<DecodeTokenCommand, DecodeTokenResult>
    {
        private readonly IJsonWebKeySetService _jwksService;

        private readonly IDataProtector _protector;

        public Decode(IJsonWebKeySetService jwksService, IDataProtectionProvider provider)
        {
            // this service provides us with rotating signing keys
            _jwksService = jwksService;

            // this service provides us with rotating encryption keys
            _protector = provider.CreateProtector($"App_{DateTime.UtcNow:yyyy-MM-dd}");
        }

        [HttpPut("api/token")]
        [SwaggerOperation(
            Summary = "Decodes a Token",
            Description = "Decodes a Token",
            OperationId = "token.decode",
            Tags = new[] {"TokenEndpoints"})
        ]
        public override async Task<ActionResult<DecodeTokenResult>> HandleAsync(
            DecodeTokenCommand request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            return Ok(DecodeJwt(request));
        }

        private DecodeTokenResult DecodeJwt(DecodeTokenCommand request)
        {
            var decodedToken = DecodeToken(request);
            var claims = decodedToken
                .Claims
                .Where(x => !x.Type.Equals("sec", StringComparison.InvariantCultureIgnoreCase))
                .ToDictionary(x => x.Type, x => x.Value);
            claims.Merge(GetPrivateClaims(decodedToken));
            return GetTokenResponse(request, claims);
        }

        private JwtSecurityToken DecodeToken(DecodeTokenCommand request)
        {
            // create a token handler
            var tokenHandler = new JwtSecurityTokenHandler();

            // return the token
            return tokenHandler.ReadJwtToken(request.Token);
        }

        private Dictionary<string, string> GetPrivateClaims(JwtSecurityToken token)
        {
            // get the secure claim
            var claim = token.Claims.FirstOrDefault(x =>
                x.Type.Equals("sec", StringComparison.InvariantCultureIgnoreCase));

            // probably deal with null exception here
            if (claim == null)
            {
                throw new SecurityTokenException(@"'sec' payload is missing from token claims.");
            }

            // convert this to bytes
            var payLoadBytes = Convert.FromBase64String(claim.Value);

            // unprotect that using the Data Protection API using today's key (see above)
            var payloadBytes = _protector.Unprotect(payLoadBytes);

            // get the payload as a json string
            var payLoad = Encoding.UTF8.GetString(payloadBytes);

            // deserialize that string
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(payLoad);
        }

        private static DecodeTokenResult GetTokenResponse(DecodeTokenCommand request, Dictionary<string, string> claims)
        {
            return new DecodeTokenResult(request.CorrelationId())
            {
                Claims = claims
            };
        }
    }
}