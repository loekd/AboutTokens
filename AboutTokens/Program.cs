using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Frontend.BFF;

namespace Frontend;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<AuthenticationStateProvider, BffAuthenticationStateProvider>();
        builder.Services.AddTransient<AntiforgeryHandler>();

        builder.Services
            .AddHttpClient("bff", client => client.BaseAddress = new Uri(builder.Configuration["BackendForFrontEnd::Authority"] ?? "https://localhost:7147"))
            .AddHttpMessageHandler<AntiforgeryHandler>();  //bff

        builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("bff"));

        await builder.Build().RunAsync();
    }
}
