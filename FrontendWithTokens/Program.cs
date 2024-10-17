using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace FrontendWithTokens
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped<CustomAuthorizationMessageHandler>();
            builder.Services
                .AddHttpClient("api", client => client.BaseAddress = new Uri(builder.Configuration["Backend::Endpoint"] ?? "https://localhost:7290"))
                .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("api"));

            builder.Services.AddOidcAuthentication(options =>
            {
                builder.Configuration.Bind("IdentityServer", options.ProviderOptions);

                options.ProviderOptions.DefaultScopes.Add("openid");
                options.ProviderOptions.DefaultScopes.Add("profile");
                options.ProviderOptions.DefaultScopes.Add("offline_access");
                options.ProviderOptions.DefaultScopes.Add("Inventory.ReadWrite");
            });

            await builder.Build().RunAsync();
        }
    }

    public class CustomAuthorizationMessageHandler : AuthorizationMessageHandler
    {
        public CustomAuthorizationMessageHandler(IAccessTokenProvider provider,
            NavigationManager navigationManager, IConfiguration configuration)
            : base(provider, navigationManager)
        {
            string apiUrl = configuration["Backend::Endpoint"] ?? "https://localhost:7290";
            ConfigureHandler(authorizedUrls: new[] { apiUrl });

        }
    }
}