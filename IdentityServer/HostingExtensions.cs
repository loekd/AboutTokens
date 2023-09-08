using Duende.IdentityServer.Test;
using IdentityServer.Data;
using IdentityServer.Models;
using IdentityServer.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

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
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();


        ////allow X-Forward headers to specify the real host and protocol of Identity Server
        //builder.Services.Configure<ForwardedHeadersOptions>(options =>
        //{
        //    options.ForwardedHeaders =
        //        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
        //    options.KnownNetworks.Clear();
        //    options.KnownProxies.Clear();
        //    options.RequireHeaderSymmetry = false;
        //    options.ForwardLimit = null;
        //});




        builder.Services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                options.EmitStaticAudienceClaim = false;
                options.AccessTokenJwtType = "JWT";
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddAspNetIdentity<ApplicationUser>()
            .AddProfileService<CustomProfileService>()


            .AddInMemoryApiResources(Config.ApiResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.GetClients(idsrvConfig!));

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                //allow the frontend with tokens
                policy.WithOrigins("https://localhost:7267")
                .AllowAnyHeader()
                .AllowAnyMethod();
            });
        });

        return builder;
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.EnsureSeedData();

        app.UseForwardedHeaders();
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            //app.UseSwagger();
            //app.UseSwaggerUI();
        }

        //app.UseHttpsRedirection();
        app.UseCors();

        app.UseStaticFiles();
        app.UseRouting();

        app.UseIdentityServer();

        app.UseAuthorization();

        app.MapRazorPages()
            .RequireAuthorization();

        return app;
    }
}
