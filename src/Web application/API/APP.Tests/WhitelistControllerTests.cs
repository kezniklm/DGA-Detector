using System.Net;
using System.Net.Http.Json;
using APP.Deserializers;
using APP.DTOs;
using BL.Models.Whitelist;
using MongoDB.Bson;
using Xunit;

namespace APP.Tests;

[Collection("APP.Tests")]
public class WhitelistControllerTests : IAsyncLifetime
{
    private readonly ApiApplicationFactory<Program> _application;
    private readonly HttpClient _client;

    public WhitelistControllerTests(ApiApplicationFactory<Program> application)
    {
        _application = application;
        _client = _application.CreateClientWithSession();
    }

    public async Task InitializeAsync() => await _application?.SeedDatabaseAsync()!;
    public async Task DisposeAsync() => await _application.ClearMongoDbDataAsync();

    [Fact]
    public async Task GetAll_ReturnsEmptyListWhenNoData()
    {
        // Arrange
        await _application.ClearMongoDbDataAsync();
        const string url = "/Whitelist/";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        IList<WhitelistModel>? whitelists = await response.Content.ReadFromJsonAsync<IList<WhitelistModel>>();

        // Assert
        Assert.NotNull(whitelists);
        Assert.Empty(whitelists);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        // Arrange
        const string url = "/Whitelist/";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        IList<WhitelistModel>? whitelists = await response.Content.ReadFromJsonAsync<IList<WhitelistModel>>();

        // Assert
        Assert.NotNull(whitelists);
        Assert.True(whitelists.Count > 0);
    }

    [Fact]
    public async Task Get_ReturnsOkResult()
    {
        // Arrange
        const string id = "5f1a5151b5b5b5b5b5b5b5b7";
        const string url = $"/Whitelist/{id}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);
        WhitelistDto? whitelistDto = await response.Content.ReadFromJsonAsync<WhitelistDto>();
        WhitelistModel whitelist =
            WhitelistModelDeserializer.DeserializeWhitelistModel(whitelistDto ?? throw new InvalidOperationException());

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(whitelist);
        Assert.Equal(id, whitelist.Id.ToString());
    }

    [Fact]
    public async Task Get_ReturnsNotFoundResult()
    {
        // Arrange
        const string id = "5f1a5151b5b5b5b5b5b5b5b6";
        const string url = $"/Whitelist/{id}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        // Arrange
        WhitelistModel newWhitelist = new()
        {
            DomainName = "examplegood.com", Added = DateTime.Now, Id = ObjectId.GenerateNewId()
        };
        const string url = "/Whitelist/";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, newWhitelist);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        // Arrange
        const string idToDelete = "5f1a5151b5b5b5b5b5b5b5b7";
        const string url = $"/Whitelist/{idToDelete}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetWithPaginationAndFilter_ReturnsCorrectData()
    {
        // Arrange
        const int max = 10;
        const int page = 1;
        const string filter = "example";
        string url = $"/Whitelist/{max}/{page}/{filter}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        IList<WhitelistModel>? filteredWhitelists = await response.Content.ReadFromJsonAsync<IList<WhitelistModel>>();

        // Assert
        Assert.NotNull(filteredWhitelists);
        Assert.True(filteredWhitelists.Count <= max);
    }

    [Fact]
    public async Task Create_WithoutAuthorization_ReturnsUnauthorized()
    {
        // Arrange
        WhitelistModel whitelistEntry = new()
        {
            DomainName = "examplegood.com", Added = DateTime.Now, Id = ObjectId.GenerateNewId()
        };
        HttpClient clientWithoutAuthorization = _application.CreateClient();

        // Act
        HttpResponseMessage response = await clientWithoutAuthorization.PostAsJsonAsync("/Whitelist", whitelistEntry);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsBadRequestForInvalidOperation()
    {
        // Arrange
        const string nonDeletableId = "5f1a5151b5b5b5b5b5b5b5b6";
        const string url = $"/Whitelist/{nonDeletableId}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetWithPagination_ReturnsEmptyListForOutOfRangePage()
    {
        // Arrange
        const int max = 10;
        const int page = 9999;
        string url = $"/Whitelist/{max}/{page}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        IList<WhitelistModel>? results = await response.Content.ReadFromJsonAsync<IList<WhitelistModel>>();
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task GetTotalCount_ReturnsCorrectNumber()
    {
        // Arrange
        const string url = "/Whitelist/count";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        long count = await response.Content.ReadFromJsonAsync<long>();
        Assert.True(count >= 0);
    }

    [Theory]
    [InlineData("/Whitelist")]
    [InlineData("/Whitelist/count")]
    [InlineData("/Whitelist/5f1a5151b5b5b5b5b5b5b5b1")]
    public async Task Endpoints_RequireAuthentication(string url)
    {
        // Arrange
        HttpClient clientWithoutAuthorization = _application.CreateClient();

        // Act
        HttpResponseMessage response = await clientWithoutAuthorization.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetWithPaginationAndFilter_ReturnsNoResultsForStrictFilter()
    {
        // Arrange
        const int max = 10;
        const int page = 1;
        const string strictFilter = "nonexistentkeyword";
        string url = $"/Whitelist/{max}/{page}/{strictFilter}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        IList<WhitelistModel>? filteredWhitelists = await response.Content.ReadFromJsonAsync<IList<WhitelistModel>>();

        // Assert
        Assert.NotNull(filteredWhitelists);
        Assert.Empty(filteredWhitelists);
    }
}
