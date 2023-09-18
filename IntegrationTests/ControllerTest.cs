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
                        .AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme)
                        .AddFakeJwtBearer(options =>
                        {
                            options.BearerValueType = FakeJwtBearerBearerValueType.Jwt;
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
