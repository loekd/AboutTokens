using Microsoft.AspNetCore.HttpOverrides;

namespace IdentityServer;

internal static class HostingExtensions
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        var idsrvConfig = builder.Configuration
                            .GetRequiredSection(IdentityServerOptions.ConfigurationSectionName)
                            .Get<IdentityServerOptions>()!;
        Console.WriteLine($"Identity Server Config: {idsrvConfig}");


        builder.Services.AddRazorPages();

        //allow X-Forward headers to specify the real host and protocol of Identity Server
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
            options.RequireHeaderSymmetry = false;
            options.ForwardLimit = null;
        });

        builder.Services.AddIdentityServer(options =>
            {
                options.AccessTokenJwtType = "JWT"; //Azure AD requires this
            })
            .AddInMemoryApiResources(Config.ApiResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.GetClients(idsrvConfig!));

        return builder;
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseForwardedHeaders();
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            //app.UseSwagger();
            //app.UseSwaggerUI();
        }

        //app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseRouting();

        app.UseIdentityServer();

        app.UseAuthorization();
        app.MapRazorPages().RequireAuthorization();

        return app;
    }
}
