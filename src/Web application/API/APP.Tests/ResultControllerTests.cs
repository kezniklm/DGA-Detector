/**
 * @file ResultControllerTests.cs
 *
 * @brief Contains unit tests for the ResultController class.
 *
 * This file contains unit tests for the ResultController class, which is responsible for handling results in the API. The tests cover various scenarios including fetching, creating, updating, and deleting results.
 *
 * The main functionalities of this file include:
 * - Testing the retrieval of all seeded results.
 * - Testing the retrieval of a single result by ID.
 * - Testing the creation of a new result.
 * - Testing the deletion of a result.
 * - Testing result retrieval with pagination and filtering.
 * - Testing error handling for non-existent results.
 * - Testing unauthorized access for unauthenticated users.
 * - Testing validation for bad and valid results during creation.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using System.Net;
using System.Net.Http.Json;
using BL.Models.Result;
using MongoDB.Bson;
using Xunit;

namespace APP.Tests;

/// <summary>
///     Test class for the ResultController.
/// </summary>
[Collection("APP.Tests")]
public class ResultControllerTests : IAsyncLifetime
{
    private readonly ApiApplicationFactory<Program> _application;
    private readonly HttpClient _client;

    /// <summary>
    ///     Constructor initializing the test class.
    /// </summary>
    /// <param name="application">An instance of ApiApplicationFactory.</param>
    public ResultControllerTests(ApiApplicationFactory<Program> application)
    {
        _application = application;
        _client = _application.CreateClientWithSession();
    }

    /// <inheritdoc />
    public async Task InitializeAsync() => await _application?.SeedDatabaseAsync()!;

    /// <inheritdoc />
    public async Task DisposeAsync() => await _application.ClearMongoDbDataAsync();

    /// <summary>
    ///     Tests the GetAll method to ensure it returns all seeded results.
    /// </summary>
    [Fact]
    public async Task GetAll_ReturnsAllSeededResults()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/result");
        List<ResultModel>? results = await response.Content.ReadFromJsonAsync<List<ResultModel>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(results);
        Assert.Equal(3, results.Count);
    }

    /// <summary>
    ///     Tests the GetById method to ensure it returns a single result by ID.
    /// </summary>
    [Fact]
    public async Task GetById_ReturnsSingleResult()
    {
        // Arrange
        string expectedId = "5f1a5151b5b5b5b5b5b5b5b4";

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/result/{expectedId}");
        ResultModel? result = await response.Content.ReadFromJsonAsync<ResultModel>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(expectedId, result.Id);
    }

    /// <summary>
    ///     Tests the Create method to ensure it adds a new result.
    /// </summary>
    [Fact]
    public async Task Create_AddsNewResult()
    {
        // Arrange
        ResultModel newResult = new()
        {
            DomainName = "newwebsite.com",
            DangerousBoolValue = true,
            Detected = DateTime.UtcNow,
            Id = ObjectId.GenerateNewId().ToString()
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/result", newResult);
        string? createdId = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(newResult.Id, createdId);
    }

    /// <summary>
    ///     Tests the Delete method to ensure it removes a result.
    /// </summary>
    [Fact]
    public async Task Delete_RemovesResult()
    {
        // Arrange
        string resultIdToDelete = "5f1a5151b5b5b5b5b5b5b5b4";

        // Act
        HttpResponseMessage deleteResponse = await _client.DeleteAsync($"/Result/{resultIdToDelete}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        HttpResponseMessage fetchResponse = await _client.GetAsync($"/result/{resultIdToDelete}");
        Assert.Equal(HttpStatusCode.NotFound, fetchResponse.StatusCode);
    }

    /// <summary>
    ///     Tests the GetWithPaginationAndFilter method to ensure it returns filtered results.
    /// </summary>
    [Fact]
    public async Task GetWithPaginationAndFilter_ReturnsFilteredResults()
    {
        // Arrange
        int max = 2;
        int page = 1;
        string filter = "safe";

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/result/{max}/{page}/{filter}");
        List<ResultModel>? paginatedResults = await response.Content.ReadFromJsonAsync<List<ResultModel>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paginatedResults);
    }

    /// <summary>
    ///     Tests the Get method to ensure it returns a 404 status code for a non-existent result.
    /// </summary>
    [Fact]
    public async Task Get_Returns404ForNonExistentResult()
    {
        // Arrange
        string nonExistentId = ObjectId.GenerateNewId().ToString();

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/result/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    ///     Tests the Delete method to ensure it returns a 400 status code for a non-existent result.
    /// </summary>
    [Fact]
    public async Task Delete_Returns400ForNonExistentResult()
    {
        // Arrange
        string nonExistentId = ObjectId.GenerateNewId().ToString();

        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"/result/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    ///     Tests the GetAll method to ensure it returns an unauthorized status code for an unauthenticated user.
    /// </summary>
    [Fact]
    public async Task GetAll_ReturnsUnauthorizedForUnauthenticatedUser()
    {
        // Arrange - Create client without session
        HttpClient client = _application.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/result");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    ///     Tests the Create method to ensure it returns a bad request status code for a bad result.
    /// </summary>
    [Fact]
    public async Task Create_ReturnsBadRequestForBadResult()
    {
        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/result", "");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    ///     Tests the Create method to ensure it returns an OK status code for a valid result.
    /// </summary>
    [Fact]
    public async Task Create_ReturnsOkForValidResult()
    {
        // Arrange
        ResultModel validResult = new()
        {
            DomainName = "newwebsite.com",
            DangerousBoolValue = true,
            Detected = DateTime.UtcNow,
            Id = ObjectId.GenerateNewId().ToString()
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/result", validResult);
        string? createdId = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(validResult.Id, createdId);
    }
}
