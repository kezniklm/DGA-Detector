using System.Net;
using System.Net.Http.Json;
using APP.Deserializers;
using APP.DTOs;
using BL.Models.Result;
using MongoDB.Bson;
using Xunit;

namespace APP.Tests;

[Collection("APP.Tests")]
public class ResultControllerTests : IAsyncLifetime
{
    private readonly ApiApplicationFactory<Program> _application;
    private readonly HttpClient _client;

    public ResultControllerTests(ApiApplicationFactory<Program> application)
    {
        _application = application;
        _client = _application.CreateClientWithSession();
    }

    public async Task InitializeAsync() => await _application?.SeedDatabaseAsync()!;
    public async Task DisposeAsync() => await _application.ClearMongoDbDataAsync();

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

    [Fact]
    public async Task GetById_ReturnsSingleResult()
    {
        // Arrange
        string expectedId = "5f1a5151b5b5b5b5b5b5b5b4";

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/result/{expectedId}");
        ResultDto? resultDto = await response.Content.ReadFromJsonAsync<ResultDto>();
        ResultModel result =
            ResultModelDeserializer.DeserializeResultModel(resultDto ?? throw new InvalidOperationException());

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(expectedId, result.Id.ToString());
    }

    [Fact]
    public async Task Create_AddsNewResult()
    {
        // Arrange
        ResultModel newResult = new()
        {
            DomainName = "newwebsite.com",
            DangerousBoolValue = true,
            Detected = DateTime.UtcNow,
            Id = ObjectId.GenerateNewId()
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/result", newResult);
        ObjectId createdId = await response.Content.ReadFromJsonAsync<ObjectId>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

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

    [Fact]
    public async Task NumberOfDomainsToday_ReturnsNumberOfDomains()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/result/NumberOfDomainsToday");
        long count = await response.Content.ReadFromJsonAsync<long>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(count >= 0);
    }

    [Fact]
    public async Task PositiveResultsToday_ReturnsPositiveResultsCount()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/result/PositiveResultsToday");
        long count = await response.Content.ReadFromJsonAsync<long>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(count >= 0);
    }

    [Fact]
    public async Task FilteredByBlacklist_ReturnsFilteredCount()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/result/FilteredByBlacklist");
        long count = await response.Content.ReadFromJsonAsync<long>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(count >= 0);
    }

    [Fact]
    public async Task GetTotalCount_ReturnsTotalCount()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/result/count");
        long count = await response.Content.ReadFromJsonAsync<long>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(count >= 0);
    }

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

    [Fact]
    public async Task GetAll_ReturnsUnauthorizedForUnauthenticatedUser()
    {
        // Arrange - Create client without session
        var client = _application.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/result");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequestForBadResult()
    {
        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/result", "");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsOkForValidResult()
    {
        // Arrange
        ResultModel validResult = new()
        {
            DomainName = "newwebsite.com",
            DangerousBoolValue = true,
            Detected = DateTime.UtcNow,
            Id = ObjectId.GenerateNewId()
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/result", validResult);
        ObjectId createdId = await response.Content.ReadFromJsonAsync<ObjectId>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(createdId);
    }
}
