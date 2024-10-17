using Microsoft.AspNetCore.Authentication.JwtBearer;

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
            builder.Services
                .AddAuthentication()
                .AddJwtBearer(options =>
                {
#if DEBUG
                    Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true; //don't enable this on prod
                    options.RequireHttpsMetadata = false;
                    //for debugging, put breakpoints on callbacks if needed
                    options.Events = new JwtBearerEvents
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
#endif
                    options.Authority = idsrvConfig.Authority; //identity server

                    options.TokenValidationParameters.ValidateAudience = true;
                    options.TokenValidationParameters.ValidAudience = idsrvConfig.Audience; //this api

                    options.TokenValidationParameters.ValidateIssuer = true;
                    if (!string.IsNullOrWhiteSpace(idsrvConfig.Issuers))
                    {
                        options.TokenValidationParameters.ValidIssuers = idsrvConfig.Issuers.Split(';'); //identity servers
                    }
                    else
                    {
                        options.TokenValidationParameters.ValidIssuer = idsrvConfig.Authority; //identity server
                    }
                });


            builder.Services.AddAuthorizationBuilder()
                .AddPolicy(ApiPolicyName, policy =>
                {
                    //authenticated users only
                    policy.RequireAuthenticatedUser(); 

                    string[] allowedScopeValues = idsrvConfig.RequiredReadWriteScopes.Split(';');
                    //"Inventory.All" or "Inventory.ReadWrite" scope needed
                    policy.RequireClaim("scope", allowedScopeValues); 
                });
        }
    }
}