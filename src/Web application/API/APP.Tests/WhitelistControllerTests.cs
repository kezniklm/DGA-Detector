/**
 * @file WhitelistControllerTests.cs
 *
 * @brief Contains unit tests for the WhitelistController class.
 *
 * This file contains unit tests for the WhitelistController class. The tests cover various scenarios related to retrieving, creating, updating, and deleting whitelist entries via the API endpoints. These tests ensure that the WhitelistController functions correctly and returns the expected responses for different scenarios.
 *
 * The main functionalities of this file include:
 * - Testing the GetAll endpoint to ensure it returns an empty list when there is no data.
 * - Testing the GetAll endpoint to ensure it returns a non-empty list when data is present.
 * - Testing the Get endpoint to ensure it returns the correct whitelist entry.
 * - Testing the Get endpoint to ensure it returns a NotFound status when the requested whitelist entry does not exist.
 * - Testing the Create endpoint to ensure it adds a new whitelist entry.
 * - Testing the Delete endpoint to ensure it removes the specified whitelist entry.
 * - Testing the GetWithPaginationAndFilter endpoint to ensure it returns the correct data based on pagination and filtering criteria.
 * - Testing the Create endpoint to ensure it returns Unauthorized status when called without proper authorization.
 * - Testing the Delete endpoint to ensure it returns BadRequest status when attempting to delete an invalid whitelist entry.
 * - Testing the GetWithPagination endpoint to ensure it returns an empty list when the requested page is out of range.
 * - Testing various endpoints to ensure they require authentication.
 * - Testing the GetWithPaginationAndFilter endpoint to ensure it returns no results for a strict filter that does not match any whitelist entries.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using System.Net;
using System.Net.Http.Json;
using BL.Models.Whitelist;
using MongoDB.Bson;
using Xunit;

namespace APP.Tests;

/// <summary>
///     Test class for the WhitelistController.
/// </summary>
[Collection("APP.Tests")]
public class WhitelistControllerTests : IAsyncLifetime
{
    private readonly ApiApplicationFactory<Program> _application;
    private readonly HttpClient _client;

    /// <summary>
    ///     Constructor for WhitelistControllerTests.
    /// </summary>
    /// <param name="application">The ApiApplicationFactory instance.</param>
    public WhitelistControllerTests(ApiApplicationFactory<Program> application)
    {
        _application = application;
        _client = _application.CreateClientWithSession();
    }

    /// <inheritdoc />
    public async Task InitializeAsync() => await _application?.SeedDatabaseAsync()!;

    /// <inheritdoc />
    public async Task DisposeAsync() => await _application.ClearMongoDbDataAsync();

    /// <summary>
    ///     Test method to verify GetAll method returns an empty list when no data is available.
    /// </summary>
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

    /// <summary>
    ///     Test method to verify GetAll method returns an OK result.
    /// </summary>
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

    /// <summary>
    ///     Test method to verify Get method returns an OK result.
    /// </summary>
    [Fact]
    public async Task Get_ReturnsOkResult()
    {
        // Arrange
        const string id = "5f1a5151b5b5b5b5b5b5b5b7";
        const string url = $"/Whitelist/{id}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);
        WhitelistModel? whitelist = await response.Content.ReadFromJsonAsync<WhitelistModel>();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(whitelist);
        Assert.Equal(id, whitelist.Id);
    }

    /// <summary>
    ///     Test method to verify Get method returns a NotFound result.
    /// </summary>
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

    /// <summary>
    ///     Test method to verify Create method returns an OK result.
    /// </summary>
    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        // Arrange
        WhitelistModel newWhitelist = new()
        {
            DomainName = "examplegood.com", Added = DateTime.Now, Id = ObjectId.GenerateNewId().ToString()
        };
        const string url = "/Whitelist/";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, newWhitelist);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    ///     Test method to verify Delete method returns an OK result.
    /// </summary>
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

    /// <summary>
    ///     Test method to verify GetWithPaginationAndFilter method returns correct data.
    /// </summary>
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

    /// <summary>
    ///     Test method to verify Create method returns Unauthorized when not authorized.
    /// </summary>
    [Fact]
    public async Task Create_WithoutAuthorization_ReturnsUnauthorized()
    {
        // Arrange
        WhitelistModel whitelistEntry = new()
        {
            DomainName = "examplegood.com", Added = DateTime.Now, Id = ObjectId.GenerateNewId().ToString()
        };
        HttpClient clientWithoutAuthorization = _application.CreateClient();

        // Act
        HttpResponseMessage response = await clientWithoutAuthorization.PostAsJsonAsync("/Whitelist", whitelistEntry);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    ///     Test method to verify Delete method returns BadRequest for an invalid operation.
    /// </summary>
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

    /// <summary>
    ///     Test method to verify GetWithPagination method returns an empty list for an out-of-range page.
    /// </summary>
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

    /// <summary>
    ///     Test method to verify that endpoints require authentication.
    /// </summary>
    /// <param name="url">The URL of the endpoint.</param>
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

    /// <summary>
    ///     Test method to verify GetWithPaginationAndFilter method returns no results for a strict filter.
    /// </summary>
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
