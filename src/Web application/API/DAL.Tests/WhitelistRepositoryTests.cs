/**
 * @file WhitelistRepositoryTests.cs
 *
 * @brief Contains unit tests for the WhitelistRepository class.
 *
 * This file contains unit tests for the WhitelistRepository class, which is responsible for handling data operations related to whitelisted entities. It tests various functionalities of the repository, including adding, updating, deleting, and querying whitelist entities.
 *
 * The main functionalities tested in this file include:
 * - Retrieving all whitelist entities asynchronously.
 * - Inserting a new whitelist entity asynchronously.
 * - Updating an existing whitelist entity asynchronously.
 * - Removing a whitelist entity asynchronously.
 * - Searching for whitelist entities by name asynchronously.
 * - Handling exceptions such as duplicate key errors and entity not found errors.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 */

using Common.Exceptions;
using DAL.Entities;
using DAL.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace DAL.Tests;

/// <summary>
///     Test class for testing the functionality of the WhitelistRepository.
/// </summary>
public class WhitelistRepositoryTests
{
    private readonly ApiDbContext _dbContext;
    private readonly Mock<IMongoClient> _mockClient;
    private readonly Mock<IMongoCollection<WhitelistEntity>> _mockCollection;
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly RepositoryBase<WhitelistEntity> _repository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WhitelistRepositoryTests" /> class.
    /// </summary>
    public WhitelistRepositoryTests()
    {
        _mockClient = new Mock<IMongoClient>();
        _mockDatabase = new Mock<IMongoDatabase>();
        _mockCollection = new Mock<IMongoCollection<WhitelistEntity>>();
        _mockClient.Setup(c => c.GetDatabase(It.IsAny<string>(), null)).Returns(_mockDatabase.Object);
        _mockDatabase.Setup(d => d.GetCollection<WhitelistEntity>(It.IsAny<string>(), null))
            .Returns(_mockCollection.Object);
        _dbContext = new ApiDbContext(_mockClient.Object, "testDatabase");
        _repository = new RepositoryBase<WhitelistEntity>(_dbContext);
    }

    /// <summary>
    ///     Tests the GetAllAsync method of the repository to ensure it returns all entities.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ReturnsAllEntities()
    {
        // Arrange
        Mock<IAsyncCursor<WhitelistEntity>> mockCursor = new();
        List<WhitelistEntity> testEntities = new();

        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(testEntities);
        _mockCollection
            .Setup(x => x.FindAsync(It.IsAny<FilterDefinition<WhitelistEntity>>(),
                It.IsAny<FindOptions<WhitelistEntity, WhitelistEntity>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        IList<WhitelistEntity> results = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(testEntities.Count, results.Count);
    }

    /// <summary>
    ///     Tests the InsertAsync method of the repository to ensure it adds a new entity and returns its ID.
    /// </summary>
    [Fact]
    public async Task InsertAsync_AddsNewEntity_ReturnsId()
    {
        // Arrange
        string id = ObjectId.GenerateNewId().ToString();
        WhitelistEntity testEntity = new() { Id = id, DomainName = "New Entity", Added = DateTime.Now };
        _mockCollection.Setup(x => x.InsertOneAsync(testEntity, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask).Verifiable();

        // Act
        string resultId = await _repository.InsertAsync(testEntity);

        // Assert
        Assert.Equal(testEntity.Id, resultId);
        _mockCollection.Verify();
    }

    /// <summary>
    ///     Tests the UpdateAsync method of the repository to ensure it updates an existing entity and returns its ID.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_UpdatesEntity_ReturnsId()
    {
        // Arrange
        string id = ObjectId.GenerateNewId().ToString();
        WhitelistEntity testEntity = new() { Id = id, DomainName = "Updated Name", Added = DateTime.Now };
        ReplaceOneResult.Acknowledged replaceResult = new(1, 1, id);
        _mockCollection.Setup(x => x.ReplaceOneAsync(It.IsAny<FilterDefinition<WhitelistEntity>>(), testEntity,
            It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(replaceResult);

        // Act
        string? resultId = await _repository.UpdateAsync(testEntity);

        // Assert
        Assert.NotNull(resultId);
        Assert.Equal(id, resultId);
    }

    /// <summary>
    ///     Tests the RemoveAsync method of the repository to ensure it deletes an entity successfully.
    /// </summary>
    [Fact]
    public async Task RemoveAsync_DeletesEntity_Successfully()
    {
        // Arrange
        string id = ObjectId.GenerateNewId().ToString();
        DeleteResult.Acknowledged deleteResult = new(1);
        _mockCollection.Setup(x =>
                x.DeleteOneAsync(It.IsAny<FilterDefinition<WhitelistEntity>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult);

        // Act
        await _repository.RemoveAsync(id);

        // Assert
        _mockCollection.Verify(
            x => x.DeleteOneAsync(It.IsAny<FilterDefinition<WhitelistEntity>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    ///     Tests the RemoveAsync method of the repository to ensure it throws an exception when the entity does not exist.
    /// </summary>
    [Fact]
    public async Task RemoveAsync_ThrowsInvalidDeleteException_WhenEntityDoesNotExist()
    {
        // Arrange
        string id = ObjectId.GenerateNewId().ToString();
        DeleteResult.Acknowledged deleteResult = new(0);
        _mockCollection.Setup(x =>
                x.DeleteOneAsync(It.IsAny<FilterDefinition<WhitelistEntity>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidDeleteException>(async () => await _repository.RemoveAsync(id));
    }

    /// <summary>
    ///     Tests the SearchByNameAsync method of the repository to ensure it returns matching entities when name matches.
    /// </summary>
    [Fact]
    public async Task SearchByNameAsync_ReturnsMatchingEntities_WhenNameMatches()
    {
        // Arrange
        string nameToSearch = "test";
        List<WhitelistEntity> testEntities = new()
        {
            new WhitelistEntity
            {
                DomainName = nameToSearch, Added = DateTime.Now, Id = ObjectId.GenerateNewId().ToString()
            }
        };
        Mock<IAsyncCursor<WhitelistEntity>> mockCursor = new();
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(testEntities);
        _mockCollection.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<WhitelistEntity>>(),
                It.IsAny<FindOptions<WhitelistEntity, WhitelistEntity>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        IList<WhitelistEntity> results = await _repository.SearchByNameAsync(nameToSearch);

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results);
    }

    /// <summary>
    ///     Tests the GetAllAsync method of the repository to ensure it returns an empty list when the collection is empty.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenCollectionIsEmpty()
    {
        // Arrange
        Mock<IAsyncCursor<WhitelistEntity>> mockCursor = new();
        List<WhitelistEntity> testEntities = new();
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(testEntities);
        _mockCollection
            .Setup(x => x.FindAsync(It.IsAny<FilterDefinition<WhitelistEntity>>(),
                It.IsAny<FindOptions<WhitelistEntity, WhitelistEntity>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        IList<WhitelistEntity> results = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    /// <summary>
    ///     Tests the InsertAsync method of the repository to ensure it throws an exception when the entity already exists.
    /// </summary>
    [Fact]
    public async Task InsertAsync_ThrowsException_WhenEntityExists()
    {
        // Arrange
        string id = ObjectId.GenerateNewId().ToString();
        WhitelistEntity testEntity = new() { Id = id, DomainName = "Existing Entity", Added = DateTime.Now };
        _mockCollection.Setup(x => x.InsertOneAsync(testEntity, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MongoException("Duplicate key error."));

        // Act & Assert
        await Assert.ThrowsAsync<MongoException>(() => _repository.InsertAsync(testEntity));
    }

    /// <summary>
    ///     Tests the SearchByNameAsync method of the repository to ensure it returns an empty list when no matches are found.
    /// </summary>
    [Fact]
    public async Task SearchByNameAsync_ReturnsEmptyList_WhenNoMatches()
    {
        // Arrange
        string nameToSearch = "NonExistent";
        List<WhitelistEntity> testEntities = new();
        Mock<IAsyncCursor<WhitelistEntity>> mockCursor = new();
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(testEntities);
        _mockCollection.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<WhitelistEntity>>(),
                It.IsAny<FindOptions<WhitelistEntity, WhitelistEntity>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        IList<WhitelistEntity> results = await _repository.SearchByNameAsync(nameToSearch);

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    /// <summary>
    ///     Tests the UpdateAsync method of the repository to ensure it returns null when the entity does not exist.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenEntityDoesNotExist()
    {
        // Arrange
        string id = ObjectId.GenerateNewId().ToString();
        WhitelistEntity testEntity = new() { Id = id, DomainName = "Non-Existing Name", Added = DateTime.Now };
        ReplaceOneResult.Acknowledged replaceResult = new(0, 0, id);
        _mockCollection.Setup(x => x.ReplaceOneAsync(It.IsAny<FilterDefinition<WhitelistEntity>>(), testEntity,
            It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(replaceResult);

        // Act
        string? resultId = await _repository.UpdateAsync(testEntity);

        // Assert
        Assert.Null(resultId);
    }

    /// <summary>
    ///     Tests the GetMaxOrGetAllAsync method of the repository to ensure it throws an exception when max is invalid.
    /// </summary>
    [Fact]
    public async Task GetMaxOrGetAllAsync_ThrowsArgumentOutOfRangeException_WhenMaxIsInvalid() =>
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _repository.GetLimitOrGetAllAsync(-1, 0));

    /// <summary>
    ///     Tests the GetMaxOrGetAllAsync method of the repository to ensure it throws an exception when page is invalid.
    /// </summary>
    [Fact]
    public async Task GetMaxOrGetAllAsync_ThrowsArgumentOutOfRangeException_WhenPageIsInvalid() =>
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _repository.GetLimitOrGetAllAsync(5, -1));
}
