using IdentityModel.Client;
using InventoryApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace InventoryApi.Controllers
{
    /// <summary>
    /// This code demonstrates how tokens are validated. 
    /// Don't use this code in production, 
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TokenValidationController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IdentityServerOptions _idpConfig;

        public TokenValidationController(IHttpClientFactory httpClientFactory, IOptions<IdentityServerOptions> idpOptions)
        {
            if (httpClientFactory is null)
            {
                throw new ArgumentNullException(nameof(httpClientFactory));
            }

            if (idpOptions is null || idpOptions.Value is null)
            {
                throw new ArgumentNullException(nameof(idpOptions));
            }

            _httpClient = httpClientFactory.CreateClient("Idp");
            _idpConfig = idpOptions.Value;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TokenModel token)
        {
            var validationResult = await ValidateToken(token.Token);
            return Ok(new { validationResult.IsValid, validationResult.Message });
        }


        private async Task<(bool IsValid, string Message)> ValidateToken(string token)
        {
            bool tokenIsValid = false;

            // Fetch OIDC Discovery Document
            var discoveryDocument = await FetchDiscoveryDocument();
            string message;
            if (discoveryDocument != null && discoveryDocument.JwksUri != null)
            {
                // Retrieve the JSON Web Key Set (JWKS) URL from the Discovery Document
                string jwksEndpoint = discoveryDocument.JwksUri;

                // Fetch JSON Web Key Set (JWKS)
                var jwks = await GetJwksAsync(jwksEndpoint); //should be cached for offline token validation in later calls

                if (jwks != null)
                {
                    // Create a TokenValidationParameters object using the same values as we use in " builder.Services.AddAuthentication("Bearer").AddJwtBearer"
                    var validationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKeys = jwks.Keys, //use the manually downloaded keyset, this happens under water in  builder.Services.AddAuthentication("Bearer")

                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ValidAudience = _idpConfig.Audience
                    };
                    if (!string.IsNullOrWhiteSpace(_idpConfig.Issuers))
                    {
                        validationParameters.ValidIssuers = _idpConfig.Issuers.Split(';');
                    }
                    else
                    {
                        validationParameters.ValidIssuer = _idpConfig.Authority;
                    }

                    // Create a JwtSecurityTokenHandler
                    var tokenHandler = new JwtSecurityTokenHandler();

                    try
                    {
                        // Validate the JWT token using the provided public keys, throws exception for invalid tokens
                        _ = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                        // JWT is valid; you can access claims from the validatedToken and claimsPrincipal
                        tokenIsValid = true;
                        message = "Token is valid";
                    }
                    catch (SecurityTokenValidationException ex)
                    {
                        message = ex.Message;
                    }
                }
                else
                {
                    message = "Failed to get JWKS";
                }
            }
            else
            {
                message = "Failed to get discovery document";
            }
            return new(tokenIsValid, message);
        }

        /// <summary>
        /// Fetches IDP metadata from public endpoint.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task<DiscoveryDocumentResponse?> FetchDiscoveryDocument()
        {
            var disco = await _httpClient.GetDiscoveryDocumentAsync();
            if (disco.IsError)
            {
                Console.WriteLine("Failed to fetch discovery document from Identity Server. {0}", disco.Error);
                return null;
            }
            return disco;
        }

        // Helper method to fetch JSON Web Key Set (JWKS)
        private async Task<JsonWebKeySet?> GetJwksAsync(string jwksEndpoint)
        {
            var response = await _httpClient.GetAsync(jwksEndpoint);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var keySet = JsonWebKeySet.Create(content);
                return keySet;
            }
            return null;
        }
    }
}
