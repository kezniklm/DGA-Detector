/**
 * @file ApiApplicationFactory.cs
 *
 * @brief Provides a factory for creating an API application with integrated MongoDB container for testing purposes.
 *
 * This class extends WebApplicationFactory<TEntryPoint> and implements IAsyncLifetime to create an API application instance for testing purposes. It includes functionality to initialize the MongoDB container, register users, authenticate sessions, dispose of resources, and clear MongoDB data between tests.
 *
 * The main functionalities of this class include:
 * - Initializing MongoDB container and client.
 * - Registering users and authenticating sessions.
 * - Configuring services and disposing resources.
 * - Clearing MongoDB data between tests.
 * - Seeding MongoDB database with test data.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using System.Text;
using System.Text.Json;
using DAL;
using DAL.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using Xunit;

namespace APP.Tests;

/// <summary>
///     Factory for creating a test application with MongoDB setup.
/// </summary>
/// <typeparam name="TEntryPoint">The entry point of the application.</typeparam>
public class ApiApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IAsyncLifetime
    where TEntryPoint : class
{
    private readonly HttpClient _clientWithSession;
    private readonly MongoClient? _mongoClient;
    private readonly MongoDbContainer? _mongoDbContainer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiApplicationFactory{TEntryPoint}" /> class.
    /// </summary>
    public ApiApplicationFactory()
    {
        _mongoDbContainer = new MongoDbBuilder()
            .WithName("mongodb")
            .WithPortBinding(27017, 27019)
            .Build();

        _mongoDbContainer.StartAsync().GetAwaiter().GetResult();

        _mongoClient = new MongoClient(_mongoDbContainer.GetConnectionString());
        EnsureDatabaseCreated();

        _clientWithSession = CreateClient();
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        await RegisterUserAsync();
        await AuthenticateAndCaptureSessionAsync();
    }

    /// <inheritdoc />
    public new async Task DisposeAsync() => await ClearMongoDbDataAsync();

    /// <summary>
    ///     Configures the host for the web application.
    /// </summary>
    /// <param name="builder">The web host builder.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls("http://localhost:5000", "https://localhost:5001");

        builder.ConfigureServices(services =>
        {
            ServiceDescriptor? userDbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<UserDbContext>));

            if (userDbContextDescriptor != null)
            {
                services.Remove(userDbContextDescriptor);
            }

            services.AddScoped<ApiDbContext>(_ =>
                new ApiDbContext(_mongoDbContainer?.GetConnectionString() ?? throw new InvalidOperationException(),
                    "Database"));
            services.AddDbContext<UserDbContext>(options => { options.UseInMemoryDatabase("UserInMemoryDB"); });
        });

        base.ConfigureWebHost(builder);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _mongoClient?.Cluster?.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    ///     Clears data from the MongoDB database.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ClearMongoDbDataAsync()
    {
        IMongoDatabase? database = _mongoClient?.GetDatabase("Database");

        List<string>? collectionNames = await database?.ListCollectionNames().ToListAsync()!;

        foreach (string? collectionName in collectionNames)
        {
            await database.DropCollectionAsync(collectionName);
        }
    }

    /// <summary>
    ///     Authenticates and captures the session for testing purposes.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task AuthenticateAndCaptureSessionAsync()
    {
        var loginModel = new { email = "test@user.com", password = "Test123@" };
        StringContent content = new(JsonSerializer.Serialize(loginModel), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _clientWithSession.PostAsync("/login?useSessionCookies=true", content);
        response.EnsureSuccessStatusCode();

        if (response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? cookies))
        {
            _clientWithSession.DefaultRequestHeaders.Add("Cookie", string.Join(";", cookies));
        }
    }

    /// <summary>
    ///     Registers a user for testing purposes.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task RegisterUserAsync()
    {
        var registerModel = new { email = "test@user.com", password = "Test123@" };
        StringContent content = new(JsonSerializer.Serialize(registerModel), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _clientWithSession.PostAsync("/register", content);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    ///     Creates an HTTP client with an authenticated session.
    /// </summary>
    /// <returns>The HTTP client with an authenticated session.</returns>
    public HttpClient CreateClientWithSession() => _clientWithSession;

    /// <summary>
    ///     Ensures that the MongoDB database is created if it does not exist.
    /// </summary>
    private void EnsureDatabaseCreated()
    {
        IMongoDatabase? database = _mongoClient?.GetDatabase("Database");
        if (database is null)
        {
            _mongoClient?.GetDatabase("admin")
                .RunCommand(new JsonCommand<BsonDocument>("{ createDatabase: 'Database' }"));
        }
    }

    /// <summary>
    ///     Seeds the MongoDB database with initial data.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SeedDatabaseAsync()
    {
        IMongoDatabase? database = _mongoClient?.GetDatabase("Database");

        List<BlacklistEntity> blacklistSeeds = new()
        {
            new BlacklistEntity
            {
                Added = DateTime.UtcNow.AddDays(-10),
                DomainName = "badwebsite.com",
                Id = "5f1a5151b5b5b5b5b5b5b5b1"
            },
            new BlacklistEntity
            {
                Added = DateTime.UtcNow.AddDays(-5),
                DomainName = "maliciouswebsite.com",
                Id = "5f1a5151b5b5b5b5b5b5b5b2"
            },
            new BlacklistEntity
            {
                Added = DateTime.UtcNow.AddDays(-3),
                DomainName = "phishingwebsite.com",
                Id = "5f1a5151b5b5b5b5b5b5b5b3"
            }
        };

        List<ResultEntity> resultSeeds = new()
        {
            new ResultEntity
            {
                Detected = DateTime.UtcNow,
                DidBlacklistHit = true,
                DangerousProbabilityValue = 0.9,
                DangerousBoolValue = true,
                DomainName = "badwebsite.com",
                Id = "5f1a5151b5b5b5b5b5b5b5b4"
            },
            new ResultEntity
            {
                Detected = DateTime.UtcNow.AddDays(-1),
                DidBlacklistHit = false,
                DangerousProbabilityValue = 0.2,
                DangerousBoolValue = false,
                DomainName = "safewebsite.com",
                Id = "5f1a5151b5b5b5b5b5b5b5b5"
            },
            new ResultEntity
            {
                Detected = DateTime.UtcNow.AddDays(-2),
                DidBlacklistHit = true,
                DangerousProbabilityValue = 0.95,
                DangerousBoolValue = true,
                DomainName = "verybadwebsite.com",
                Id = "5f1a5151b5b5b5b5b5b5b5b6"
            }
        };

        List<WhitelistEntity> whitelistSeeds = new()
        {
            new WhitelistEntity
            {
                Added = DateTime.UtcNow.AddDays(-10),
                DomainName = "goodwebsite.com",
                Id = "5f1a5151b5b5b5b5b5b5b5b7"
            },
            new WhitelistEntity
            {
                Added = DateTime.UtcNow.AddDays(-5),
                DomainName = "trustedsite.com",
                Id = "5f1a5151b5b5b5b5b5b5b5b8"
            },
            new WhitelistEntity
            {
                Added = DateTime.UtcNow.AddDays(-3),
                DomainName = "securewebsite.com",
                Id = "5f1a5151b5b5b5b5b5b5b5b9"
            }
        };

        if (database != null)
        {
            IMongoCollection<BlacklistEntity>? blacklistCollection =
                database.GetCollection<BlacklistEntity>("Blacklist");
            await blacklistCollection.InsertManyAsync(blacklistSeeds);

            IMongoCollection<ResultEntity>? resultCollection = database.GetCollection<ResultEntity>("Result");
            await resultCollection.InsertManyAsync(resultSeeds);

            IMongoCollection<WhitelistEntity>? whitelistCollection =
                database.GetCollection<WhitelistEntity>("Whitelist");
            await whitelistCollection.InsertManyAsync(whitelistSeeds);
        }
    }
}
