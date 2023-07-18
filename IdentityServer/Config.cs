using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityModel;
using Microsoft.AspNetCore.DataProtection;
using Secret = Duende.IdentityServer.Models.Secret;

namespace IdentityServer;

public static class Config
{
    public const string ScreeningReadWriteScope = "Screening.ReadWrite";
    public const string OnboardingReadWriteScope = "Onboarding.ReadWrite";

    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource()
            {
                Name = "verification",
                UserClaims = new List<string>
                {
                    JwtClaimTypes.Email,
                    JwtClaimTypes.EmailVerified
                }
            }
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope(ScreeningReadWriteScope, "Screening Api Scope")
            {
                Description = "Access Screening API."
            },
            new ApiScope(OnboardingReadWriteScope, "Onboarding Api Scope")
            {
                Description = "Access Onboarding API."
            }
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new List<ApiResource>
        {
            new ApiResource("ScreeningAPI", "Screening Api Resource")
            {
                Description = "Screening API resource",
                Scopes = { ScreeningReadWriteScope },
                UserClaims = { "name", "email", "role" },
            },
            new ApiResource("api://AzureADTokenExchange", "Onboarding Api Resource")
            {
                Description = "Onboarding API resource",
                Scopes = { OnboardingReadWriteScope },
                UserClaims = { "name", "email", "role" },
            }
        };

    public static IEnumerable<Client> GetClients(IdentityServerOptions config)
    {
        return new List<Client>
        {
            // machine-to-machine client to protect the screening api
            new Client
            {
                ClientId = "screeningapi",
                ClientSecrets = { new Secret(config.ClientSecret.Sha256()) },

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                // scopes that client has access to
                AllowedScopes = { ScreeningReadWriteScope }
            },
            // machine-to-machine client to access the onboarding api
            new Client
            {
                ClientId = "onboardingapi",
                ClientSecrets = { new Secret(config.ClientSecret.Sha256()) },

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                // scopes that client has access to
                AllowedScopes = { OnboardingReadWriteScope },
                ClientClaimsPrefix = string.Empty,
                Claims =
                {
                    new ClientClaim(JwtClaimTypes.Subject, config.ImpersonationIdentityObjectId)
                }
            }
        };
    }
}
