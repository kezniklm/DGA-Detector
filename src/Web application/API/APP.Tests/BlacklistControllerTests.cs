/**
 * @file BlacklistControllerTests.cs
 *
 * @brief Provides test cases for the BlacklistController class.
 *
 * This file contains test cases for the BlacklistController class, which handles operations related to blacklisted domains. It tests various functionalities of the controller, such as retrieving blacklisted domains, adding new entries, deleting entries, and filtering results.
 *
 * The main functionalities tested in this file include:
 * - Retrieving all blacklisted domains.
 * - Adding new blacklisted domains.
 * - Deleting existing blacklisted domains.
 * - Filtering blacklisted domains based on various criteria.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using System.Net;
using System.Net.Http.Json;
using BL.Models.Blacklist;
using MongoDB.Bson;
using Xunit;

namespace APP.Tests;

/// <summary>
///     Test class for the BlacklistController.
/// </summary>
[Collection("APP.Tests")]
public class BlacklistControllerTests : IAsyncLifetime
{
    private readonly ApiApplicationFactory<Program> _application;
    private readonly HttpClient _client;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlacklistControllerTests" /> class.
    /// </summary>
    /// <param name="application">Instance of ApiApplicationFactory for the test application.</param>
    public BlacklistControllerTests(ApiApplicationFactory<Program> application)
    {
        _application = application;
        _client = _application.CreateClientWithSession();
    }

    /// <inheritdoc />
    public async Task InitializeAsync() => await _application?.SeedDatabaseAsync()!;

    /// <inheritdoc />
    public async Task DisposeAsync() => await _application.ClearMongoDbDataAsync();

    /// <summary>
    ///     Test case for GetAll method when there is no data.
    /// </summary>
    [Fact]
    public async Task GetAll_ReturnsEmptyListWhenNoData()
    {
        // Arrange
        await _application.ClearMongoDbDataAsync();
        const string url = "/Blacklist/";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        IList<BlacklistModel>? blacklists = await response.Content.ReadFromJsonAsync<IList<BlacklistModel>>();

        // Assert
        Assert.NotNull(blacklists);
        Assert.Empty(blacklists);
    }

    /// <summary>
    ///     Test case for GetAll method when there is data present.
    /// </summary>
    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        // Arrange
        const string url = "/Blacklist/";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        IList<BlacklistModel>? blacklists = await response.Content.ReadFromJsonAsync<IList<BlacklistModel>>();

        // Assert
        Assert.NotNull(blacklists);
        Assert.True(blacklists.Count > 0);
    }

    /// <summary>
    ///     Test case for retrieving a blacklist entry by ID.
    /// </summary>
    [Fact]
    public async Task Get_ReturnsOkResult()
    {
        // Arrange
        const string id = "5f1a5151b5b5b5b5b5b5b5b1";
        const string url = $"/Blacklist/{id}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);
        BlacklistModel? blacklist = await response.Content.ReadFromJsonAsync<BlacklistModel>();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(blacklist);
        Assert.Equal(id, blacklist.Id);
    }

    /// <summary>
    ///     Test case for retrieving a blacklist entry by ID when the entry does not exist.
    /// </summary>
    [Fact]
    public async Task Get_ReturnsNotFoundResult()
    {
        // Arrange
        const string id = "5f1a5151b5b5b5b5b5b5b5b9";
        const string url = $"/Blacklist/{id}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    ///     Test case for creating a new blacklist entry.
    /// </summary>
    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        // Arrange
        BlacklistModel newBlacklist = new()
        {
            DomainName = "examplebad.com", Added = DateTime.Now, Id = ObjectId.GenerateNewId().ToString()
        };
        const string url = "/Blacklist/";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, newBlacklist);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    ///     Test case for deleting a blacklist entry.
    /// </summary>
    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        // Arrange
        const string idToDelete = "5f1a5151b5b5b5b5b5b5b5b1";
        const string url = $"/Blacklist/{idToDelete}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    ///     Test case for retrieving blacklist entries with pagination and filter.
    /// </summary>
    [Fact]
    public async Task GetWithPaginationAndFilter_ReturnsCorrectData()
    {
        // Arrange
        const int max = 10;
        const int page = 1;
        const string filter = "example";
        string url = $"/Blacklist/{max}/{page}/{filter}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        IList<BlacklistModel>? filteredBlacklists = await response.Content.ReadFromJsonAsync<IList<BlacklistModel>>();

        // Assert
        Assert.NotNull(filteredBlacklists);
        Assert.True(filteredBlacklists.Count <= max);
    }

    /// <summary>
    ///     Test case for creating a blacklist entry without authorization.
    /// </summary>
    [Fact]
    public async Task Create_WithoutAuthorization_ReturnsUnauthorized()
    {
        // Arrange
        BlacklistModel blacklistEntry = new()
        {
            DomainName = "examplebad.com", Added = DateTime.Now, Id = ObjectId.GenerateNewId().ToString()
        };
        HttpClient clientWithoutAuthorization = _application.CreateClient();

        // Act
        HttpResponseMessage response = await clientWithoutAuthorization.PostAsJsonAsync("/Blacklist", blacklistEntry);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    ///     Test case for deleting a blacklist entry with an invalid operation.
    /// </summary>
    [Fact]
    public async Task Delete_ReturnsBadRequestForInvalidOperation()
    {
        // Arrange
        const string nonDeletableId = "5f1a5151b5b5b5b5b5b5b5b9";
        const string url = $"/Blacklist/{nonDeletableId}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    ///     Test case for retrieving blacklist entries with pagination and filter for an out-of-range page.
    /// </summary>
    [Fact]
    public async Task GetWithPagination_ReturnsEmptyListForOutOfRangePage()
    {
        // Arrange
        const int max = 10;
        const int page = 9999;
        string url = $"/Blacklist/{max}/{page}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        IList<BlacklistModel>? results = await response.Content.ReadFromJsonAsync<IList<BlacklistModel>>();
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    /// <summary>
    ///     Test case for ensuring authentication is required for specified endpoints.
    /// </summary>
    /// <param name="url">The URL of the endpoint to test.</param>
    [Theory]
    [InlineData("/Blacklist")]
    [InlineData("/Blacklist/count")]
    [InlineData("/Blacklist/5f1a5151b5b5b5b5b5b5b5b1")]
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
    ///     Test case for retrieving blacklist entries with pagination and strict filter, returning no results.
    /// </summary>
    [Fact]
    public async Task GetWithPaginationAndFilter_ReturnsNoResultsForStrictFilter()
    {
        // Arrange
        const int max = 10;
        const int page = 1;
        const string strictFilter = "nonexistentkeyword";
        string url = $"/Blacklist/{max}/{page}/{strictFilter}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        IList<BlacklistModel>? filteredBlacklists = await response.Content.ReadFromJsonAsync<IList<BlacklistModel>>();

        // Assert
        Assert.NotNull(filteredBlacklists);
        Assert.Empty(filteredBlacklists);
    }
}
