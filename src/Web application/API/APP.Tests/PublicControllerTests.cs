using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace APP.Tests;

[Collection("APP.Tests")]
public class PublicControllerTests : IAsyncLifetime
{
    private readonly ApiApplicationFactory<Program> _application;
    private readonly HttpClient _client;

    public PublicControllerTests(ApiApplicationFactory<Program> application)
    {
        _application = application;
        _client = _application.CreateClient();
    }

    public async Task InitializeAsync() => await _application?.SeedDatabaseAsync()!;
    public async Task DisposeAsync() => await _application.ClearMongoDbDataAsync();

    [Fact]
    public async Task GetTotalCountOfBlacklist_ReturnsCorrectNumber()
    {
        // Arrange
        const string url = "/Public/blacklist/count";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        long count = await response.Content.ReadFromJsonAsync<long>();
        Assert.True(count >= 0);
    }

    [Fact]
    public async Task FilteredByBlacklist_ReturnsFilteredCount()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/public/FilteredByBlacklist");
        long count = await response.Content.ReadFromJsonAsync<long>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(count >= 0);
    }

    [Fact]
    public async Task GetTotalCount_ReturnsTotalCount()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/public/results/count");
        long count = await response.Content.ReadFromJsonAsync<long>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(count >= 0);
    }

    [Fact]
    public async Task NumberOfDomainsToday_ReturnsNumberOfDomains()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/public/NumberOfDomainsToday");
        long count = await response.Content.ReadFromJsonAsync<long>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(count >= 0);
    }

    [Fact]
    public async Task PositiveResultsToday_ReturnsPositiveResultsCount()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/public/PositiveResultsToday");
        long count = await response.Content.ReadFromJsonAsync<long>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(count >= 0);
    }

    [Fact]
    public async Task GetTotalCountOfWhitelist_ReturnsCorrectNumber()
    {
        // Arrange
        const string url = "/public/whitelist/count";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        long count = await response.Content.ReadFromJsonAsync<long>();
        Assert.True(count >= 0);
    }
}
