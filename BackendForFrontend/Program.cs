using Duende.Bff;
using Duende.Bff.Yarp;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Yarp.ReverseProxy.Configuration;

namespace BackendForFrontend;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("This is the BFF app");

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            //db storage for crypto keys for auth cookie
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddDataProtection()
                .PersistKeysToDbContext<ApplicationDbContext>();

            builder.Services.AddControllers();
            builder.Services.AddRazorPages();
            builder.Services.AddBff();

            //configure routes to blazor host and api
            builder.Services.AddReverseProxy()
                .AddTransforms<AccessTokenTransformProvider>()
                .LoadFromMemory(
                    new[]
                    {
                        new RouteConfig()
                        {
                            RouteId = "InventoryApi",
                            ClusterId = "InventoryApiCluster",

                            Match = new RouteMatch
                            {
                                Path = "/api/{**catch-all}"
                            }
                        }.WithAccessToken(TokenType.User),
                        new RouteConfig()
                        {
                            AuthorizationPolicy = "Anonymous",
                            RouteId = "Blazor",
                            ClusterId = "BlazorCluster",

                            Match = new RouteMatch
                            {
                                Path = "/{**catch-all}"
                            }
                        }
                    },
                    new[]
                    {
                        new ClusterConfig
                        {
                            ClusterId = "BlazorCluster",
                            Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "Blazor", new DestinationConfig() { Address = "https://localhost:7166" } }, //blazor frontend
                            }
                        },
                        new ClusterConfig
                        {
                            ClusterId = "InventoryApiCluster",
                            Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "Inventory", new DestinationConfig() { Address = "https://localhost:7290" } }, //api
                            }
                        }
                    });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "cookie";
                options.DefaultChallengeScheme = "oidc";
                options.DefaultSignOutScheme = "oidc";
            })
                .AddCookie("cookie", options =>
                {
                    options.Cookie.Name = "__Host-blazor";
                    options.Cookie.SameSite = SameSiteMode.Strict;

                    options.Events = new ();
                    options.Events.OnRedirectToAccessDenied= ctx =>
                    {
                        return Task.CompletedTask;
                    };

                    options.Events.OnSignedIn= ctx =>
                    {
                        return Task.CompletedTask;
                    };
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = "https://localhost:7242";

                    // confidential client using code flow + PKCE
                    options.ClientId = "bff-api";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";
                    options.ResponseMode = "query";

                    options.MapInboundClaims = false;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;
                    options.UsePkce = true;

                    // request scopes + refresh tokens
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("Inventory.All");
                    options.Scope.Add("offline_access");

                    options.Events = new ();
                    options.Events.OnAuthenticationFailed = ctx =>
                    {
                        return Task.CompletedTask;
                    };

                    options.Events.OnTokenValidated = ctx =>
                    {
                        return Task.CompletedTask;
                    };
                });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseBff();
            app.UseAuthorization();

            app.MapBffManagementEndpoints();
            app.MapRazorPages();

            app.MapControllers()
                .RequireAuthorization();

            app.MapReverseProxy()
                .AsBffApiEndpoint().SkipAntiforgery();

            //create database
            using (var serviceScope = app.Services.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureCreated();
            }

            app.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            Console.WriteLine("Shut down complete");
        }

    }
}