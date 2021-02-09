using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using DataTrust.Pod.Api;
using DataTrust.Pod.Api.EndPoints.TokenEndPoints;
using Xunit;

namespace DataTrust.Pod.Api.Tests.TokenEndpoints
{
    public class CreateEndpoint : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public CreateEndpoint(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreatesANewToken()
        {
            var newToken = new CreateTokenCommand()
            {
                Address = "Test Address",
                Email = "test@test.com",
                Fullname = "Full Name",
                Telephone = "0554 454545"
            };

            var response = await _client.PostAsync($"api/token", new StringContent(JsonConvert.SerializeObject(newToken), Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();
            var stringResponse = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<CreateTokenResult>(stringResponse);

            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            
            // validate token using kwts
        }

        [Fact]
        public async Task CreatesANewValidatedToken()
        {
            var newToken = new CreateTokenCommand()
            {
                Address = "Test Address",
                Email = "test@test.com",
                Fullname = "Full Name",
                Telephone = "0554 454545"
            };

            var response = await _client.PostAsync($"api/token", new StringContent(JsonConvert.SerializeObject(newToken), Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();
            var stringResponse = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<CreateTokenResult>(stringResponse);

            Assert.NotNull(result);
            Assert.NotNull(result.Token);

            // validate token using kwts
            var jwksResponse = await _client.GetAsync("/jwks");
            jwksResponse.EnsureSuccessStatusCode();
            var stringjwksResponse = await jwksResponse.Content.ReadAsStringAsync();
            var jwksResult = JsonConvert.DeserializeObject<JsonWebKeySet>(stringjwksResponse);

            Assert.NotNull(jwksResult);

            // decode token
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
        }

        [Fact]
        public async Task GivenLongRunningCreateRequest_WhenTokenSourceCallsForCancellation_RequestIsTerminated()
        {
            // Arrange, generate a token source that times out instantly
            var tokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(0));
            var newToken = new CreateTokenCommand()
            {
                Address = "Test Address",
                Email = "test@test.com",
                Fullname = "Full Name",
                Telephone = "0554 454545"
            };

            // Act
            var request = _client.PostAsync("/Tokens", new StringContent(JsonConvert.SerializeObject(newToken), Encoding.UTF8, "application/json"), tokenSource.Token);

            // Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await request);
        }
    }
}
