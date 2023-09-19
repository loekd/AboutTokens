using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using WebMotions.Fake.Authentication.JwtBearer;
using Program = InventoryApi.Program;

namespace IntegrationTests;

public abstract class ControllerTest
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    /// <summary>
    /// Client preconfigured to call Inventory API
    /// </summary>
    protected HttpClient Client { get { return _client; } }

    protected ControllerTest()
    {
        IdentityModelEventSource.ShowPII = true;
        _factory = new WebApplicationFactory<Program>();
        _client = _factory
            .WithWebHostBuilder(builder =>
            {
                builder.UseTestServer();
                builder.ConfigureTestServices(services =>
                {
                    services
                        .AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme)  //Allow FakeBearer + token in Authorization header
                        .AddFakeJwtBearer(options =>
                        {
                            options.BearerValueType = FakeJwtBearerBearerValueType.Jwt;
                            //useful for debugging, put breakpoints on callbacks if needed
                            options.Events = new WebMotions.Fake.Authentication.JwtBearer.Events.JwtBearerEvents
                            {
                                OnAuthenticationFailed = ctx => Task.CompletedTask,
                                OnForbidden = ctx => Task.CompletedTask,
                                OnTokenValidated = ctx => Task.CompletedTask
                            };
                        });

                });
            })
            .CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }
}
