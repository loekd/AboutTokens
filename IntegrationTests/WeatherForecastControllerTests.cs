using InventoryApi;
using System.Net.Http.Json;
using static IntegrationTests.FakeTokenHelper;

namespace IntegrationTests;

[TestClass]
public class WeatherForecastControllerTests : ControllerTest
{
    [TestMethod]
    public async Task ShouldReturnStatusCodeOK_WhenAuthenticatedWithProperScopes()
    {
        //arrange
        Client.AddJwtClaims("Alice", InventoryClient, InventoryReadWriteScope);

        //act
        var response = await Client.GetAsync(InventoryEndpoint);

        //assert
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task ShouldReturnObjects_WhenAuthenticatedWithProperScopes()
    {
        //arrange
        Client.AddJwtClaims("Alice", InventoryClient, InventoryReadWriteScope);

        //act
        var result = await Client.GetFromJsonAsync<IEnumerable<WeatherForecast>>($"{InventoryEndpoint}/");

        //assert
        Assert.IsInstanceOfType<IEnumerable<WeatherForecast>>(result);
    }

    [TestMethod]
    public async Task ShouldReturnStatusCodeUnauthorized_WhenNotAuthenticated()
    {
        //arrange
        //Client.AddJwtClaims("Alice", InventoryClient, InventoryReadWriteScope); //no token

        //act
        var response = await Client.GetAsync(InventoryEndpoint);

        //assert
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task ShouldReturnStatusCodeUnauthorized_WhenAuthenticatedWithIncorrectScopes()
    {
        //arrange
        Client.AddJwtClaims("Alice", InventoryClient, "SomeOtherScope"); //valid token, wrong scope

        //act
        var response = await Client.GetAsync(InventoryEndpoint);

        //assert
        Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }
}
