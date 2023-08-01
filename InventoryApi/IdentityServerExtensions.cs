namespace InventoryApi
{

    public static class IdentityServerExtensions
    {
        private const string ApiPolicyName = "ApiPolicy";

        public static void ConfigureIdentityServer(this WebApplicationBuilder builder)
        {
            var idsrvConfig = builder.Configuration
                .GetRequiredSection(IdentityServerOptions.ConfigurationSectionName)
                .Get<IdentityServerOptions>()!;

            Console.WriteLine($"Identity Server Config: {idsrvConfig}");
            Console.WriteLine($"Environment IdentityServer__Authority: {Environment.GetEnvironmentVariable("IdentityServer__Authority")}");

            //Use JWT bearer tokens for authentication
            builder.Services.AddAuthentication("Bearer")
                        .AddJwtBearer(options =>
                        {
                            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
                            options.RequireHttpsMetadata = false;
                            options.Authority = idsrvConfig.Authority;
                            options.TokenValidationParameters.ValidateAudience = true;
                            options.TokenValidationParameters.ValidAudience = idsrvConfig.Audience;
                            if (!string.IsNullOrWhiteSpace(idsrvConfig.Issuers))
                            {
                                options.TokenValidationParameters.ValidIssuers = idsrvConfig.Issuers.Split(';');
                            }
                            else
                            {
                                options.TokenValidationParameters.ValidIssuer = idsrvConfig.Authority;
                            }
                            //options.ConfigurationManager = new CustomConfigurationManager(idsrvConfig.Authority);

                            options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents();
                            options.Events.OnAuthenticationFailed = ctx =>
                            {
                                return Task.CompletedTask;
                            };

                            options.Events.OnTokenValidated = ctx =>
                            {
                                return Task.CompletedTask;
                            };
                        });


            //Require authenticated users
            //Require read-write scope
            builder.Services.AddAuthorization(options =>
                options.AddPolicy(ApiPolicyName, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", idsrvConfig.RequiredReadWriteScopes.Split(';'));
                })
            );
        }
    }
}

//public class CustomConfigurationManager : IConfigurationManager<OpenIdConnectConfiguration>
//{
//    private readonly string authority;

//    public CustomConfigurationManager(string authority)
//    {
//        this.authority = authority;
//    }
//    public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(CancellationToken cancel)
//    {
//        var handler = new HttpClientHandler() { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator };
//        var httpClient = new HttpClient(handler);

//        var request = new HttpRequestMessage
//        {
//            RequestUri = new Uri($"{authority}/.well-known/openid-configuration"),
//            Method = HttpMethod.Get
//        };

//        var configurationResult = await httpClient.SendAsync(request, cancel);
//        var resultContent = await configurationResult.Content.ReadAsStringAsync(cancel);
//        if (configurationResult.IsSuccessStatusCode)
//        {
//            var config = OpenIdConnectConfiguration.Create(resultContent);
//            var jwks = config.JwksUri;
//            var keyRequest = new HttpRequestMessage
//            {
//                RequestUri = new Uri(jwks),
//                Method = HttpMethod.Get
//            };
//            var keysResponse = await httpClient.SendAsync(keyRequest, cancel);
//            var keysResultContent = await keysResponse.Content.ReadAsStringAsync(cancel);
//            if (keysResponse.IsSuccessStatusCode)
//            {
//                config.JsonWebKeySet = new Microsoft.IdentityModel.Tokens.JsonWebKeySet(keysResultContent);
//                var signingKeys = config.JsonWebKeySet.GetSigningKeys();
//                foreach (var key in signingKeys)
//                {
//                    config.SigningKeys.Add(key);
//                }
//            }
//            else
//            {
//                throw new Exception($"Failed to get jwks: {keysResponse.StatusCode}: {keysResultContent}");
//            }

//            return config;
//        }
//        else
//        {
//            throw new Exception($"Failed to get configuration: {configurationResult.StatusCode}: {resultContent}");
//        }
//    }

//    public void RequestRefresh()
//    {
//    }
//}