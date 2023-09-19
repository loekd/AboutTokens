using System.Net;

namespace IntegrationTests;

public static class FakeTokenHelper
{
    public const string InventoryReadWriteScope = "Inventory.ReadWrite";
    public const string InventoryAllScope = "Inventory.All";
    public const string InventoryClient = "InventoryApi";
    public const string InventoryEndpoint = "WeatherForecast";

    public const string Issuer = "https://localhost:7242";
    public const string FrontendClient = "frontend-with-tokens";

    public static HttpClient AddJwtClaims(this HttpClient httpClient, string userName, string audience, params string[] scopes)
    {
        httpClient.SetFakeJwtBearerToken(new Dictionary<string, object>
        {
            { "iss", Issuer },
            { "client_id", FrontendClient },
            { "aud", audience},
            { "sub", userName },
            { "unique_name", userName },
            { "scope", scopes},
        });
        return httpClient;
    }
}