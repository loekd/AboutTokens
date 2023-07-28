using AboutTokens;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http.Json;
using System.Net;
using System.Security.Claims;
using AboutTokens.BFF;

namespace AboutTokens
{
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
                .AddHttpClient("bff", client => client.BaseAddress = new Uri("https://localhost:7147"))
                .AddHttpMessageHandler<AntiforgeryHandler>();  //bff
            
            builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("bff"));

            await builder.Build().RunAsync();
        }
    }

    public class BffAuthenticationStateProvider : AuthenticationStateProvider
    {
        private static readonly TimeSpan UserCacheRefreshInterval = TimeSpan.FromSeconds(60);

        private readonly HttpClient _client;
        private readonly ILogger<BffAuthenticationStateProvider> _logger;

        private DateTimeOffset _userLastCheck = DateTimeOffset.FromUnixTimeSeconds(0);
        private ClaimsPrincipal _cachedUser = new(new ClaimsIdentity());

        public BffAuthenticationStateProvider(
            HttpClient client,
            ILogger<BffAuthenticationStateProvider> logger)
        {
            _client = client;
            _logger = logger;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = await GetUser();
            var state = new AuthenticationState(user);

            // checks periodically for a session state change and fires event
            // this causes a round trip to the server
            // adjust the period accordingly if that feature is needed
            if (user.Identity.IsAuthenticated)
            {
                _logger.LogInformation("starting background check..");
                Timer? timer = null;

                timer = new Timer(async _ =>
                {
                    var currentUser = await GetUser(false);
                    if (currentUser.Identity.IsAuthenticated == false)
                    {
                        _logger.LogInformation("user logged out");
                        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(currentUser)));
                        await timer.DisposeAsync();
                    }
                }, null, 1000, 5000);
            }

            return state;
        }

        private async ValueTask<ClaimsPrincipal> GetUser(bool useCache = true)
        {
            var now = DateTimeOffset.Now;
            if (useCache && now < _userLastCheck + UserCacheRefreshInterval)
            {
                _logger.LogDebug("Taking user from cache");
                return _cachedUser;
            }

            _logger.LogDebug("Fetching user");
            _cachedUser = await FetchUser();
            _userLastCheck = now;

            return _cachedUser;
        }

        record ClaimRecord(string Type, object Value);

        private async Task<ClaimsPrincipal> FetchUser()
        {
            try
            {
                _logger.LogInformation("Fetching user information from {BaseAddress}.", _client.BaseAddress);
                var response = await _client.GetAsync("bff/user?slide=false");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var claims = await response.Content.ReadFromJsonAsync<List<ClaimRecord>>();

                    var identity = new ClaimsIdentity(
                        nameof(BffAuthenticationStateProvider),
                        "name",
                        "role");

                    foreach (var claim in claims)
                    {
                        identity.AddClaim(new Claim(claim.Type, claim.Value.ToString()));
                    }

                    return new ClaimsPrincipal(identity);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fetching user failed.");
            }

            return new ClaimsPrincipal(new ClaimsIdentity());
        }
    }
}