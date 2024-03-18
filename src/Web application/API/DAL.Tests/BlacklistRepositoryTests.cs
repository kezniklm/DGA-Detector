using Common.Exceptions;
using DAL.Entities;
using DAL.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace DAL.Tests;

public class BlacklistRepositoryTests
{
    private readonly ApiDbContext _dbContext;
    private readonly Mock<IMongoClient> _mockClient;
    private readonly Mock<IMongoCollection<BlacklistEntity>> _mockCollection;
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly RepositoryBase<BlacklistEntity> _repository;

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

    [Fact]
    public async Task InsertAsync_AddsNewEntity_ReturnsId()
    {
        // Arrange
        ObjectId id = ObjectId.GenerateNewId();
        BlacklistEntity testEntity = new() { Id = id, DomainName = "New Entity", Added = DateTime.Now };
        _mockCollection.Setup(x => x.InsertOneAsync(testEntity, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask).Verifiable();

        // Act
        ObjectId resultId = await _repository.InsertAsync(testEntity);

        // Assert
        Assert.Equal(testEntity.Id, resultId);
        _mockCollection.Verify();
    }

    [Fact]
    public async Task UpdateAsync_UpdatesEntity_ReturnsId()
    {
        // Arrange
        ObjectId id = ObjectId.GenerateNewId();
        BlacklistEntity testEntity = new() { Id = id, DomainName = "Updated Name", Added = DateTime.Now };
        ReplaceOneResult.Acknowledged replaceResult = new(1, 1, id);
        _mockCollection.Setup(x => x.ReplaceOneAsync(It.IsAny<FilterDefinition<BlacklistEntity>>(), testEntity,
            It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(replaceResult);

        // Act
        ObjectId? resultId = await _repository.UpdateAsync(testEntity);

        // Assert
        Assert.NotNull(resultId);
        Assert.Equal(id, resultId);
    }

    [Fact]
    public async Task RemoveAsync_DeletesEntity_Successfully()
    {
        // Arrange
        ObjectId id = ObjectId.GenerateNewId();
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

    [Fact]
    public async Task RemoveAsync_ThrowsInvalidDeleteException_WhenEntityDoesNotExist()
    {
        // Arrange
        ObjectId id = ObjectId.GenerateNewId();
        DeleteResult.Acknowledged deleteResult = new(0);
        _mockCollection.Setup(x =>
                x.DeleteOneAsync(It.IsAny<FilterDefinition<BlacklistEntity>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidDeleteException>(async () => await _repository.RemoveAsync(id));
    }

    [Fact]
    public async Task SearchByNameAsync_ReturnsMatchingEntities_WhenNameMatches()
    {
        // Arrange
        const string nameToSearch = "test";
        List<BlacklistEntity> testEntities = new()
        {
            new BlacklistEntity { DomainName = nameToSearch, Added = DateTime.Now, Id = ObjectId.GenerateNewId() }
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

    [Fact]
    public async Task InsertAsync_ThrowsException_WhenEntityExists()
    {
        // Arrange
        ObjectId id = ObjectId.GenerateNewId();
        BlacklistEntity testEntity = new() { Id = id, DomainName = "Existing Entity", Added = DateTime.Now };
        _mockCollection.Setup(x => x.InsertOneAsync(testEntity, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MongoException("Duplicate key error."));

        // Act & Assert
        await Assert.ThrowsAsync<MongoException>(() => _repository.InsertAsync(testEntity));
    }

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

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenEntityDoesNotExist()
    {
        // Arrange
        ObjectId id = ObjectId.GenerateNewId();
        BlacklistEntity testEntity = new() { Id = id, DomainName = "Non-Existing Name", Added = DateTime.Now };
        ReplaceOneResult.Acknowledged replaceResult = new(0, 0, id);
        _mockCollection.Setup(x => x.ReplaceOneAsync(It.IsAny<FilterDefinition<BlacklistEntity>>(), testEntity,
            It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(replaceResult);

        // Act
        ObjectId? resultId = await _repository.UpdateAsync(testEntity);

        // Assert
        Assert.Null(resultId);
    }

    [Fact]
    public async Task GetMaxOrGetAllAsync_ThrowsArgumentOutOfRangeException_WhenMaxIsInvalid() =>
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _repository.GetLimitOrGetAllAsync(-1, 0));

    [Fact]
    public async Task GetMaxOrGetAllAsync_ThrowsArgumentOutOfRangeException_WhenPageIsInvalid() =>
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _repository.GetLimitOrGetAllAsync(5, -1));
}
