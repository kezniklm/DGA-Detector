/**
 * @file BlacklistRepositoryTests.cs
 *
 * @brief Unit tests for the BlacklistRepository class.
 *
 * This file contains unit tests for the BlacklistRepository class. The BlacklistRepository class is responsible for managing operations related to blacklisted entities in the database.
 *
 * The main functionalities tested in this file include:
 * - Retrieving all entities successfully.
 * - Adding a new entity and verifying its ID.
 * - Updating an existing entity and verifying its ID.
 * - Deleting an entity successfully.
 * - Handling exceptions when performing database operations.
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
///     Unit tests for the BlacklistRepository class.
/// </summary>
public class BlacklistRepositoryTests
{
    private readonly ApiDbContext _dbContext;
    private readonly Mock<IMongoClient> _mockClient;
    private readonly Mock<IMongoCollection<BlacklistEntity>> _mockCollection;
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly RepositoryBase<BlacklistEntity> _repository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlacklistRepositoryTests" /> class.
    /// </summary>
    public BlacklistRepositoryTests()
    {
        _mockClient = new Mock<IMongoClient>();
        _mockDatabase = new Mock<IMongoDatabase>();
        _mockCollection = new Mock<IMongoCollection<BlacklistEntity>>();
        _mockClient.Setup(c => c.GetDatabase(It.IsAny<string>(), null)).Returns(_mockDatabase.Object);
        _mockDatabase.Setup(d => d.GetCollection<BlacklistEntity>(It.IsAny<string>(), null))
            .Returns(_mockCollection.Object);
        _dbContext = new ApiDbContext(_mockClient.Object, "testDatabase");
        _repository = new RepositoryBase<BlacklistEntity>(_dbContext);
    }

    /// <summary>
    ///     Test method to verify that GetAllAsync returns all entities successfully.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ReturnsAllEntities()
    {
        // Arrange
        Mock<IAsyncCursor<BlacklistEntity>> mockCursor = new();
        List<BlacklistEntity> testEntities = new();

        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(testEntities);
        _mockCollection
            .Setup(x => x.FindAsync(It.IsAny<FilterDefinition<BlacklistEntity>>(),
                It.IsAny<FindOptions<BlacklistEntity, BlacklistEntity>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        IList<BlacklistEntity> results = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(testEntities.Count, results.Count);
    }

    /// <summary>
    ///     Test method to verify that InsertAsync adds a new entity and returns its ID.
    /// </summary>
    [Fact]
    public async Task InsertAsync_AddsNewEntity_ReturnsId()
    {
        // Arrange
        string id = ObjectId.GenerateNewId().ToString();
        BlacklistEntity testEntity = new() { Id = id, DomainName = "New Entity", Added = DateTime.Now };
        _mockCollection.Setup(x => x.InsertOneAsync(testEntity, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask).Verifiable();

        // Act
        string resultId = await _repository.InsertAsync(testEntity);

        // Assert
        Assert.Equal(testEntity.Id, resultId);
        _mockCollection.Verify();
    }

    /// <summary>
    ///     Test method to verify that UpdateAsync updates an existing entity and returns its ID.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_UpdatesEntity_ReturnsId()
    {
        // Arrange
        string id = ObjectId.GenerateNewId().ToString();
        BlacklistEntity testEntity = new() { Id = id, DomainName = "Updated Name", Added = DateTime.Now };
        ReplaceOneResult.Acknowledged replaceResult = new(1, 1, id);
        _mockCollection.Setup(x => x.ReplaceOneAsync(It.IsAny<FilterDefinition<BlacklistEntity>>(), testEntity,
            It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(replaceResult);

        // Act
        string? resultId = await _repository.UpdateAsync(testEntity);

        // Assert
        Assert.NotNull(resultId);
        Assert.Equal(id, resultId);
    }

    /// <summary>
    ///     Test method to verify that RemoveAsync deletes an entity successfully.
    /// </summary>
    [Fact]
    public async Task RemoveAsync_DeletesEntity_Successfully()
    {
        // Arrange
        string id = ObjectId.GenerateNewId().ToString();
        DeleteResult.Acknowledged deleteResult = new(1);
        _mockCollection.Setup(x =>
                x.DeleteOneAsync(It.IsAny<FilterDefinition<BlacklistEntity>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult);

        // Act
        await _repository.RemoveAsync(id);

        // Assert
        _mockCollection.Verify(
            x => x.DeleteOneAsync(It.IsAny<FilterDefinition<BlacklistEntity>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    ///     Test method to verify that RemoveAsync throws InvalidDeleteException when the entity does not exist.
    /// </summary>
    [Fact]
    public async Task RemoveAsync_ThrowsInvalidDeleteException_WhenEntityDoesNotExist()
    {
        // Arrange
        string id = ObjectId.GenerateNewId().ToString();
        DeleteResult.Acknowledged deleteResult = new(0);
        _mockCollection.Setup(x =>
                x.DeleteOneAsync(It.IsAny<FilterDefinition<BlacklistEntity>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidDeleteException>(async () => await _repository.RemoveAsync(id));
    }

    /// <summary>
    ///     Test method to verify that SearchByNameAsync returns matching entities when the name matches.
    /// </summary>
    [Fact]
    public async Task SearchByNameAsync_ReturnsMatchingEntities_WhenNameMatches()
    {
        // Arrange
        const string nameToSearch = "test";
        List<BlacklistEntity> testEntities = new()
        {
            new BlacklistEntity
            {
                DomainName = nameToSearch, Added = DateTime.Now, Id = ObjectId.GenerateNewId().ToString()
            }
        };
        Mock<IAsyncCursor<BlacklistEntity>> mockCursor = new();
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(testEntities);
        _mockCollection.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<BlacklistEntity>>(),
                It.IsAny<FindOptions<BlacklistEntity, BlacklistEntity>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        IList<BlacklistEntity> results = await _repository.SearchByNameAsync(nameToSearch);

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results);
    }

    /// <summary>
    ///     Test method to verify that GetAllAsync returns an empty list when the collection is empty.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenCollectionIsEmpty()
    {
        // Arrange
        Mock<IAsyncCursor<BlacklistEntity>> mockCursor = new();
        List<BlacklistEntity> testEntities = new();
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(testEntities);
        _mockCollection
            .Setup(x => x.FindAsync(It.IsAny<FilterDefinition<BlacklistEntity>>(),
                It.IsAny<FindOptions<BlacklistEntity, BlacklistEntity>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        IList<BlacklistEntity> results = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    /// <summary>
    ///     Test method to verify that InsertAsync throws an exception when the entity already exists.
    /// </summary>
    [Fact]
    public async Task InsertAsync_ThrowsException_WhenEntityExists()
    {
        // Arrange
        string id = ObjectId.GenerateNewId().ToString();
        BlacklistEntity testEntity = new() { Id = id, DomainName = "Existing Entity", Added = DateTime.Now };
        _mockCollection.Setup(x => x.InsertOneAsync(testEntity, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MongoException("Duplicate key error."));

        // Act & Assert
        await Assert.ThrowsAsync<MongoException>(() => _repository.InsertAsync(testEntity));
    }

    /// <summary>
    ///     Test method to verify that SearchByNameAsync returns an empty list when there are no matches.
    /// </summary>
    [Fact]
    public async Task SearchByNameAsync_ReturnsEmptyList_WhenNoMatches()
    {
        // Arrange
        const string nameToSearch = "NonExistent";
        List<BlacklistEntity> testEntities = new();
        Mock<IAsyncCursor<BlacklistEntity>> mockCursor = new();
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(testEntities);
        _mockCollection.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<BlacklistEntity>>(),
                It.IsAny<FindOptions<BlacklistEntity, BlacklistEntity>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        IList<BlacklistEntity> results = await _repository.SearchByNameAsync(nameToSearch);

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    /// <summary>
    ///     Test method to verify that UpdateAsync returns null when the entity does not exist.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenEntityDoesNotExist()
    {
        // Arrange
        string id = ObjectId.GenerateNewId().ToString();
        BlacklistEntity testEntity = new() { Id = id, DomainName = "Non-Existing Name", Added = DateTime.Now };
        ReplaceOneResult.Acknowledged replaceResult = new(0, 0, id);
        _mockCollection.Setup(x => x.ReplaceOneAsync(It.IsAny<FilterDefinition<BlacklistEntity>>(), testEntity,
            It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(replaceResult);

        // Act
        string? resultId = await _repository.UpdateAsync(testEntity);

        // Assert
        Assert.Null(resultId);
    }

    /// <summary>
    ///     Test method to verify that GetMaxOrGetAllAsync throws ArgumentOutOfRangeException when max is invalid.
    /// </summary>
    [Fact]
    public async Task GetMaxOrGetAllAsync_ThrowsArgumentOutOfRangeException_WhenMaxIsInvalid() =>
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _repository.GetLimitOrGetAllAsync(-1, 0));

    /// <summary>
    ///     Test method to verify that GetMaxOrGetAllAsync throws ArgumentOutOfRangeException when page is invalid.
    /// </summary>
    [Fact]
    public async Task GetMaxOrGetAllAsync_ThrowsArgumentOutOfRangeException_WhenPageIsInvalid() =>
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _repository.GetLimitOrGetAllAsync(5, -1));
}
