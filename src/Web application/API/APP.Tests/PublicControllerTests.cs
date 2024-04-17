/**
 * @file PublicControllerTests.cs
 *
 * @brief Contains unit tests for the PublicController class.
 *
 * This file contains unit tests for the PublicController class. The PublicController is responsible for handling public API endpoints related to blacklists, whitelists, and domain statistics. These tests validate the behavior of various endpoints by sending HTTP requests and asserting the returned results.
 *
 * The main functionalities of this file include:
 * - Testing the correct behavior of endpoints related to blacklist operations.
 * - Testing the correct behavior of endpoints related to statistics retrieval.
 * - Ensuring that endpoints return expected HTTP status codes and response formats.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace APP.Tests;

/// <summary>
///     Contains unit tests for the <see cref="PublicController" /> class.
/// </summary>
/// <remarks>
///     This class contains unit tests for the <see cref="PublicController" /> class. It utilizes XUnit for testing and the
///     <see cref="HttpClient" /> for sending HTTP requests to API endpoints. The tests validate the behavior of various
///     endpoints related to blacklists, whitelists, and domain statistics.
/// </remarks>
[Collection("APP.Tests")]
public class PublicControllerTests : IAsyncLifetime
{
    private readonly ApiApplicationFactory<Program> _application;
    private readonly HttpClient _client;

    /// <summary>
    ///     Constructs an instance of the <see cref="PublicControllerTests" /> class.
    /// </summary>
    /// <param name="application">The API application factory used for testing.</param>
    public PublicControllerTests(ApiApplicationFactory<Program> application)
    {
        _application = application;
        _client = _application.CreateClient();
    }

    /// <inheritdoc />
    public async Task InitializeAsync() => await _application?.SeedDatabaseAsync()!;

    /// <inheritdoc />
    public async Task DisposeAsync() => await _application.ClearMongoDbDataAsync();

    /// <summary>
    ///     Tests whether the method GetTotalCountOfBlacklist returns the correct number.
    /// </summary>
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

    /// <summary>
    ///     Tests whether the method FilteredByBlacklist returns the filtered count.
    /// </summary>
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

    /// <summary>
    ///     Tests whether the method GetTotalCount returns the total count.
    /// </summary>
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

    /// <summary>
    ///     Tests whether the method NumberOfDomainsToday returns the number of domains.
    /// </summary>
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

    /// <summary>
    ///     Tests whether the method PositiveResultsToday returns the count of positive results.
    /// </summary>
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

    /// <summary>
    ///     Tests whether the method GetTotalCountOfWhitelist returns the correct number.
    /// </summary>
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
