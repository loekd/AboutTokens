using System.Net;

namespace IntegrationTests;

public static class FakeTokenHelper
{
    public const string InventoryReadWriteScope = "Inventory.ReadWrite";
    public const string InventoryAllScope = "Inventory.All";
    public const string InventoryClient = "inventory-api";
    public const string InventoryEndpoint = "WeatherForecast";

    public static HttpClient AddJwtClaims(this HttpClient client, string userName, string audience, params string[] scopes)
    {
        client.SetFakeJwtBearerToken(new Dictionary<string, object>
        {
            { "aud", audience},
            { "sub", userName },
            { "unique_name", userName },
            { "scope", scopes},
        });
        return client;
    }
}