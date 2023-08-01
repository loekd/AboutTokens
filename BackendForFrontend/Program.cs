using Duende.Bff;
using Duende.Bff.Yarp;
using Microsoft.AspNetCore.Authorization;
using Yarp.ReverseProxy.Configuration;

namespace BackendForFrontend;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddRazorPages();
            builder.Services.AddBff();
            builder.Services.AddReverseProxy()
                .AddTransforms<AccessTokenTransformProvider>()
                .LoadFromMemory(
                    new[]
                    {
                        new RouteConfig()
                        {
                            RouteId = "InventoryApi",
                            ClusterId = "InventoryApiCluster",
                            Transforms = new[]
                            {
                                new Dictionary<string, string>
                                {
                                    { "PathRemovePrefix", "/api" }
                                }
                            },

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
                                { "Blazor", new DestinationConfig() { Address = "https://localhost:7166" } },
                            }
                        },
                        new ClusterConfig
                        {
                            ClusterId = "InventoryApiCluster",
                            Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "Inventory", new DestinationConfig() { Address = "https://localhost:7290" } },
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

            //app.UseStaticFiles();

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