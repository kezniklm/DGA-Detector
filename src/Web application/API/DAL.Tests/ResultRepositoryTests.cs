/**
 * @file ResultRepositoryTests.cs
 *
 * @brief Contains unit tests for the ResultRepository class.
 *
 * This file contains unit tests for the ResultRepository class, which is responsible for handling data operations related to ResultEntity objects. It tests various functionalities of the repository, including retrieving all entities, inserting new entities, updating existing entities, removing entities, searching by name, and handling exceptions.
 *
 * The main functionalities tested in this file include:
 * - Retrieving all ResultEntity entities asynchronously.
 * - Inserting a new ResultEntity asynchronously and verifying its ID.
 * - Updating an existing ResultEntity asynchronously and verifying its ID.
 * - Removing a ResultEntity asynchronously.
 * - Searching for ResultEntity entities by name asynchronously.
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
///     Contains unit tests for the ResultRepository class.
/// </summary>
public class ResultRepositoryTests
{
    private readonly ApiDbContext _dbContext;
    private readonly Mock<IMongoClient> _mockClient;
    private readonly Mock<IMongoCollection<ResultEntity>> _mockCollection;
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly RepositoryBase<ResultEntity> _repository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ResultRepositoryTests" /> class.
    /// </summary>
    public ResultRepositoryTests()
    {
        _mockClient = new Mock<IMongoClient>();
        _mockDatabase = new Mock<IMongoDatabase>();
        _mockCollection = new Mock<IMongoCollection<ResultEntity>>();
        _mockClient.Setup(c => c.GetDatabase(It.IsAny<string>(), null)).Returns(_mockDatabase.Object);
        _mockDatabase.Setup(d => d.GetCollection<ResultEntity>(It.IsAny<string>(), null))
            .Returns(_mockCollection.Object);
        _dbContext = new ApiDbContext(_mockClient.Object, "testDatabase");
        _repository = new RepositoryBase<ResultEntity>(_dbContext);
    }

    /// <summary>
    ///     Tests the GetAllAsync method.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ReturnsAllEntities()
    {
        // Arrange
        Mock<IAsyncCursor<ResultEntity>> mockCursor = new();
        List<ResultEntity> testEntities = new();

        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(testEntities);
        _mockCollection
            .Setup(x => x.FindAsync(It.IsAny<FilterDefinition<ResultEntity>>(),
                It.IsAny<FindOptions<ResultEntity, ResultEntity>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        IList<ResultEntity> results = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(testEntities.Count, results.Count);
    }

    /// <summary>
    ///     Tests the InsertAsync method.
    /// </summary>
    [Fact]
    public async Task InsertAsync_AddsNewEntity_ReturnsId()
    {
        // Arrange
        string id = ObjectId.GenerateNewId().ToString();
        ResultEntity testEntity = new() { Id = id, DomainName = "New Entity" };
        _mockCollection.Setup(x => x.InsertOneAsync(testEntity, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask).Verifiable();

        // Act
        string resultId = await _repository.InsertAsync(testEntity);

        // Assert
        Assert.Equal(testEntity.Id, resultId);
        _mockCollection.Verify();
    }

    /// <summary>
    ///     Tests the UpdateAsync method.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_UpdatesEntity_ReturnsId()
    {
        // Arrange
        string id = ObjectId.GenerateNewId().ToString();
        ResultEntity testEntity = new() { Id = id, DomainName = "Updated Name" };
        ReplaceOneResult.Acknowledged replaceResult = new(1, 1, id);
        _mockCollection.Setup(x => x.ReplaceOneAsync(It.IsAny<FilterDefinition<ResultEntity>>(), testEntity,
            It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(replaceResult);

        // Act
        string? resultId = await _repository.UpdateAsync(testEntity);

        // Assert
        Assert.NotNull(resultId);
        Assert.Equal(id, resultId);
    }

    /// <summary>
    ///     Tests the RemoveAsync method when deleting an existing entity.
    /// </summary>
    [Fact]
    public async Task RemoveAsync_DeletesEntity_Successfully()
    {
        // Arrange
        string id = ObjectId.GenerateNewId().ToString();
        DeleteResult.Acknowledged deleteResult = new(1);
        _mockCollection.Setup(x =>
                x.DeleteOneAsync(It.IsAny<FilterDefinition<ResultEntity>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult);

        // Act
        await _repository.RemoveAsync(id);

        // Assert
        _mockCollection.Verify(
            x => x.DeleteOneAsync(It.IsAny<FilterDefinition<ResultEntity>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    ///     Tests the RemoveAsync method when an entity to delete does not exist.
    /// </summary>
    [Fact]
    public async Task RemoveAsync_ThrowsInvalidDeleteException_WhenEntityDoesNotExist()
    {
        // Arrange
        string id = ObjectId.GenerateNewId().ToString();
        DeleteResult.Acknowledged deleteResult = new(0);
        _mockCollection.Setup(x =>
                x.DeleteOneAsync(It.IsAny<FilterDefinition<ResultEntity>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidDeleteException>(async () => await _repository.RemoveAsync(id));
    }

    /// <summary>
    ///     Tests the SearchByNameAsync method when entities with matching names exist.
    /// </summary>
    [Fact]
    public async Task SearchByNameAsync_ReturnsMatchingEntities_WhenNameMatches()
    {
        // Arrange
        string nameToSearch = "test";
        List<ResultEntity> testEntities = new() { Mock.Of<ResultEntity>(entity => entity.DomainName == nameToSearch) };
        Mock<IAsyncCursor<ResultEntity>> mockCursor = new();
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(testEntities);
        _mockCollection.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<ResultEntity>>(),
                It.IsAny<FindOptions<ResultEntity, ResultEntity>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        IList<ResultEntity> results = await _repository.SearchByNameAsync(nameToSearch);

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results);
    }

    /// <summary>
    ///     Tests the GetAllAsync method when the collection is empty.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenCollectionIsEmpty()
    {
        // Arrange
        Mock<IAsyncCursor<ResultEntity>> mockCursor = new();
        List<ResultEntity> testEntities = new();
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(testEntities);
        _mockCollection
            .Setup(x => x.FindAsync(It.IsAny<FilterDefinition<ResultEntity>>(),
                It.IsAny<FindOptions<ResultEntity, ResultEntity>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        IList<ResultEntity> results = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    /// <summary>
    ///     Tests the InsertAsync method when attempting to insert a duplicate entity.
    /// </summary>
    [Fact]
    public async Task InsertAsync_ThrowsException_WhenEntityExists()
    {
        // Arrange
        string id = ObjectId.GenerateNewId().ToString();
        ResultEntity testEntity = new() { Id = id, DomainName = "Existing Entity" };
        _mockCollection.Setup(x => x.InsertOneAsync(testEntity, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MongoException("Duplicate key error."));

        // Act & Assert
        await Assert.ThrowsAsync<MongoException>(() => _repository.InsertAsync(testEntity));
    }

    /// <summary>
    ///     Tests the SearchByNameAsync method when no matching entities are found.
    /// </summary>
    [Fact]
    public async Task SearchByNameAsync_ReturnsEmptyList_WhenNoMatches()
    {
        // Arrange
        string nameToSearch = "NonExistent";
        List<ResultEntity> testEntities = new();
        Mock<IAsyncCursor<ResultEntity>> mockCursor = new();
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(testEntities);
        _mockCollection.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<ResultEntity>>(),
                It.IsAny<FindOptions<ResultEntity, ResultEntity>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        IList<ResultEntity> results = await _repository.SearchByNameAsync(nameToSearch);

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    /// <summary>
    ///     Tests the UpdateAsync method when attempting to update a non-existing entity.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenEntityDoesNotExist()
    {
        // Arrange
        string id = ObjectId.GenerateNewId().ToString();
        ResultEntity testEntity = new() { Id = id, DomainName = "Non-Existing Name" };
        ReplaceOneResult.Acknowledged replaceResult = new(0, 0, id);
        _mockCollection.Setup(x => x.ReplaceOneAsync(It.IsAny<FilterDefinition<ResultEntity>>(), testEntity,
            It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(replaceResult);

        // Act
        string? resultId = await _repository.UpdateAsync(testEntity);

        // Assert
        Assert.Null(resultId);
    }

    /// <summary>
    ///     Tests the GetMaxOrGetAllAsync method when the provided maximum value is invalid.
    /// </summary>
    [Fact]
    public async Task GetMaxOrGetAllAsync_ThrowsArgumentOutOfRangeException_WhenMaxIsInvalid() =>
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _repository.GetLimitOrGetAllAsync(-1, 0));

    /// <summary>
    ///     Tests the GetMaxOrGetAllAsync method when the provided page value is invalid.
    /// </summary>
    [Fact]
    public async Task GetMaxOrGetAllAsync_ThrowsArgumentOutOfRangeException_WhenPageIsInvalid() =>
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _repository.GetLimitOrGetAllAsync(5, -1));
}
