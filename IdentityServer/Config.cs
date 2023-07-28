using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
using IdentityServer.Options;
using static System.Net.WebRequestMethods;
using Secret = Duende.IdentityServer.Models.Secret;

namespace IdentityServer;

public static class Config
{
    public const string InventoryReadWriteScope = "Inventory.ReadWrite";
    public const string InventoryAllScope = "Inventory.All";
    public const string OrderReadWriteScope = "Order.ReadWrite";
    public const string OrderAllScope = "Order.All";

    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope(InventoryReadWriteScope)
            {
                Description = "Read & Write Access Inventory API."
            },
            new ApiScope(InventoryAllScope)
            {
                Description = "Full Access Inventory API."
            },
            new ApiScope(OrderReadWriteScope)
            {
                Description = "Read & Write Access Order API."
            },
            new ApiScope(OrderAllScope)
            {
                Description = "Full Access Order API."
            }
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new List<ApiResource>
        {
            new ApiResource("OrderApi", "Order Api Resource")
            {
                Description = "Order API resource",
                Scopes = { OrderReadWriteScope, OrderAllScope },
                UserClaims = { "name", "email", "role" },
            },
            new ApiResource("InventoryApi", "Inventory Api Resource")
            {
                Description = "Inventory API resource",
                Scopes = { InventoryReadWriteScope, InventoryAllScope },
                UserClaims = { "name", "email", "role" },
            }
        };

    public static IEnumerable<TestUser> TestUsers =>
        new List<TestUser>
        {
            new TestUser { Username = "bob", Password = "bob" }
        };

    public static IEnumerable<Client> GetClients(IdentityServerOptions config)
    {
        return new List<Client>
        {
            // Order API
            new Client
            {
                ClientId = "order-api",
                ClientSecrets = { new Secret(config.ClientSecret.Sha256()) },

                AllowedGrantTypes = new[] {"urn:ietf:params:oauth:grant-type:token-exchange"},
                // scopes that client has access to
                AllowedScopes = { OrderReadWriteScope }
            },
            // Inventory API
            new Client
            {
                ClientId = "inventory-api",
                ClientSecrets = { new Secret(config.ClientSecret.Sha256()) },

                AllowedGrantTypes = new[] {"urn:ietf:params:oauth:grant-type:token-exchange"},
                // scopes that client has access to
                AllowedScopes = { InventoryReadWriteScope }
            },
           // Backend for frontend
            new Client
            {
                ClientId = "bff-api",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,

                RedirectUris = { "https://localhost:7147/signin-oidc", "https://localhost:7166/signin-oidc", "https://oauth.pstmn.io/v1/browser-callback", "https://oauth.pstmn.io/v1/callback" },

                BackChannelLogoutUri = "https://localhost:7166/logout",

                PostLogoutRedirectUris = { "https://localhost:7166/signout-callback-oidc" },

                AllowOfflineAccess = true,
                AllowedScopes = 
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    OrderAllScope,
                    InventoryAllScope
                }
            },
        };
    }
}
