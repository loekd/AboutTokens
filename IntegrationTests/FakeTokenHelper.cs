using System.Net;

namespace IntegrationTests;

public static class FakeTokenHelper
{
    public const string Issuer = "https://unit-test-issuer";
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