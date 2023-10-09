using InventoryApi.Models;
using System.Net.Http.Json;

namespace IntegrationTests;

[TestClass]
public class InventoryControllerTests : ControllerTest
{
    private const string InventoryReadWriteScope = "Inventory.ReadWrite";
    private const string InventoryAllScope = "Inventory.All";
    private const string InventoryClient = "InventoryApi";
    private const string InventoryEndpoint = "api/inventory";

    [TestMethod]
    public async Task ShouldReturnStatusCodeOK_WhenAuthenticatedWithProperScopes()
    {
        //arrange
        Client.AddJwtClaims("Alice", InventoryClient, InventoryReadWriteScope, "Finance.Admin", "Shipping.ReadWrite", "People.Admin", "Sales.All", "Invoice.Admin", "Orders.ReadWrite", "Payroll.Everything");

        //act
        var response = await Client.GetAsync(InventoryEndpoint);

        //assert
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task ShouldReturnStatusCodeOK_WhenAuthenticatedWithProperScope1()
    {
        //arrange
        Client.AddJwtClaims("Alice", InventoryClient, InventoryReadWriteScope);

        //act
        var response = await Client.GetAsync(InventoryEndpoint);

        //assert
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
    [TestMethod]
    public async Task ShouldReturnStatusCodeOK_WhenAuthenticatedWithProperScope2()
    {
        //arrange
        Client.AddJwtClaims("Alice", InventoryClient, InventoryAllScope);

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
        var result = await Client.GetFromJsonAsync<IEnumerable<Inventory>>($"{InventoryEndpoint}/");

        //assert
        Assert.IsInstanceOfType<IEnumerable<Inventory>>(result);
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
