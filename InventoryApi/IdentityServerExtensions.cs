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
#if DEBUG
                            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true; //don't enable this on prod
#endif
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

                            options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                            {
                                OnAuthenticationFailed = ctx =>
                                {
                                    return Task.CompletedTask;
                                },

                                OnTokenValidated = ctx =>
                                {
                                    return Task.CompletedTask;
                                },

                                OnForbidden = ctx =>
                                {
                                    return Task.CompletedTask;
                                }
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