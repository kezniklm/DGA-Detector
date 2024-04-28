/**
 * @file BlacklistFacadeTests.cs
 *
 * @brief Unit tests for the BlacklistFacade class.
 *
 * This file contains unit tests for the BlacklistFacade class, which handles the business logic for managing blacklist entities. The BlacklistFacade interacts with the data access layer to perform CRUD operations and utilizes data mapping to convert between entity models and view models.
 *
 * The main functionalities tested in this file include:
 * - Retrieving all blacklist entries successfully.
 * - Retrieving a specific blacklist entry by ID.
 * - Adding a new blacklist entry and returning its ID.
 * - Updating an existing blacklist entry based on ID.
 * - Deleting a blacklist entry.
 * - Paginating blacklist entries and applying optional date filters.
 * - Handling creations or updates based on entity existence.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-28
 * @copyright Copyright (c) 2024
 *
 */

using AutoMapper;
using BL.Facades;
using BL.Models.Blacklist;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Moq;
using Xunit;

namespace BL.Tests;

/// <summary>
///     Unit tests for the BlacklistFacade class.
/// </summary>
public class BlacklistFacadeTests
{
    private readonly BlacklistFacade _facade;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IBlacklistRepository> _repositoryMock;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlacklistFacadeTests" /> class.
    /// </summary>
    public BlacklistFacadeTests()
    {
        _repositoryMock = new Mock<IBlacklistRepository>();
        _mapperMock = new Mock<IMapper>();
        _facade = new BlacklistFacade(_repositoryMock.Object, _mapperMock.Object);
    }

    /// <summary>
    ///     Test method to verify that GetAllAsync retrieves all entries correctly and maps them successfully.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ReturnsCorrectData()
    {
        // Arrange
        List<BlacklistEntity> entities = new() { new Mock<BlacklistEntity>().Object };
        List<BlacklistModel> models = new()
        {
            new BlacklistModel { Id = "1", DomainName = "example.com", Added = DateTime.Now }
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(entities);
        _mapperMock.Setup(m => m.Map<List<BlacklistModel>>(entities)).Returns(models);

        // Act
        List<BlacklistModel> blacklist = await _facade.GetAllAsync();

        // Assert
        Assert.Equal(models.Count, blacklist.Count);
        Assert.Equal(models[0].Id, blacklist[0].Id);
    }

    /// <summary>
    ///     Test method to ensure GetByIdAsync correctly returns a model if the entity exists.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ReturnsModel_IfExists()
    {
        // Arrange
        string id = "1";
        BlacklistEntity entity = new Mock<BlacklistEntity>().Object;
        BlacklistModel model = new() { Id = id, DomainName = "example.com", Added = DateTime.Now };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
        _mapperMock.Setup(m => m.Map<BlacklistModel>(entity)).Returns(model);

        // Act
        BlacklistModel? blacklist = await _facade.GetByIdAsync(id);

        // Assert
        Assert.NotNull(blacklist);
        Assert.Equal(id, blacklist.Id);
    }

    /// <summary>
    ///     Test method to verify that CreateAsync adds a new blacklist entry and returns the created ID.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ReturnsCreatedId()
    {
        // Arrange
        BlacklistModel model = new() { Id = "1", DomainName = "newsite.com", Added = DateTime.Now };
        BlacklistEntity entity = new Mock<BlacklistEntity>().Object;
        _mapperMock.Setup(m => m.Map<BlacklistEntity>(It.IsAny<BlacklistModel>())).Returns(entity);
        _repositoryMock.Setup(r => r.InsertAsync(entity)).ReturnsAsync("1");

        // Act
        string blacklist = await _facade.CreateAsync(model);

        // Assert
        Assert.Equal("1", blacklist);
    }

    /// <summary>
    ///     Test method to check that UpdateAsync updates an existing blacklist entry and returns the updated ID.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ReturnsUpdatedId()
    {
        // Arrange
        BlacklistModel model = new() { Id = "1", DomainName = "updatedsite.com", Added = DateTime.Now };
        BlacklistEntity entity = new Mock<BlacklistEntity>().Object;
        _mapperMock.Setup(m => m.Map<BlacklistEntity>(It.IsAny<BlacklistModel>())).Returns(entity);
        _repositoryMock.Setup(r => r.UpdateAsync(entity)).ReturnsAsync("1");

        // Act
        string? blacklist = await _facade.UpdateAsync(model);

        // Assert
        Assert.Equal("1", blacklist);
    }

    /// <summary>
    ///     Test method to ensure DeleteAsync successfully calls the repository's RemoveAsync method once.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_CallsRepositoryRemove()
    {
        // Arrange
        string id = "1";
        _repositoryMock.Setup(r => r.RemoveAsync(id)).Returns(Task.CompletedTask);

        // Act
        await _facade.DeleteAsync(id);

        // Assert
        _repositoryMock.Verify(r => r.RemoveAsync(id), Times.Once);
    }

    /// <summary>
    ///     Test method to verify that GetEntriesPerPageAsync returns paginated results correctly.
    /// </summary>
    [Fact]
    public async Task GetEntriesPerPageAsync_ReturnsPaginatedBlacklists()
    {
        // Arrange
        List<BlacklistEntity> entities =
            new() { new Mock<BlacklistEntity>().Object, new Mock<BlacklistEntity>().Object };
        List<BlacklistModel> models = new()
        {
            new BlacklistModel { Id = "1", DomainName = "example.com", Added = DateTime.Now },
            new BlacklistModel { Id = "2", DomainName = "example.org", Added = DateTime.Now }
        };
        _repositoryMock.Setup(r => r.GetLimitOrGetAllAsync(0, 1, null)).ReturnsAsync(entities.Take(1));
        _mapperMock.Setup(m => m.Map<List<BlacklistModel>>(It.IsAny<IEnumerable<BlacklistEntity>>()))
            .Returns(models.Take(1).ToList());

        // Act
        List<BlacklistModel> blacklist = await _facade.GetEntriesPerPageAsync(1, 1);

        // Assert
        Assert.Single(blacklist);
        Assert.Equal("1", blacklist[0].Id);
    }

    /// <summary>
    ///     Test method to confirm that GetEntriesPerPageAsync correctly filters results by date when specified.
    /// </summary>
    [Fact]
    public async Task GetEntriesPerPageAsync_WithDateFilter_ReturnsFilteredBlacklists()
    {
        // Arrange
        DateTime startTime = DateTime.Now.AddDays(-1);
        DateTime endTime = DateTime.Now;
        List<BlacklistEntity> entities = new() { new Mock<BlacklistEntity>().Object };
        List<BlacklistModel> models = new()
        {
            new BlacklistModel { Id = "1", DomainName = "example.com", Added = DateTime.Now }
        };
        _repositoryMock.Setup(r => r.GetLimitOrGetAllAsync(0, 10, null)).ReturnsAsync(entities);
        _mapperMock.Setup(m => m.Map<List<BlacklistModel>>(It.IsAny<IEnumerable<BlacklistEntity>>())).Returns(models);

        // Act
        List<BlacklistModel> blacklist = await _facade.GetEntriesPerPageAsync(1, 10, null, startTime, endTime);

        // Assert
        Assert.Single(blacklist);
        Assert.Equal("1", blacklist[0].Id);
    }

    /// <summary>
    ///     Test method to verify that GetNumberOfAllAsync returns the correct count of all entries.
    /// </summary>
    [Fact]
    public async Task GetNumberOfAllAsync_ReturnsCorrectCount()
    {
        // Arrange
        _repositoryMock.Setup(r => r.CountAllAsync()).ReturnsAsync(10);

        // Act
        long count = await _facade.GetNumberOfAllAsync();

        // Assert
        Assert.Equal(10, count);
    }

    /// <summary>
    ///     Test method to check CreateOrUpdateAsync updates an entry when it already exists.
    /// </summary>
    [Fact]
    public async Task CreateOrUpdateAsync_UpdatesWhenExists()
    {
        // Arrange
        BlacklistModel model = new() { Id = "1", DomainName = "updatedsite.com", Added = DateTime.Now };
        _repositoryMock.Setup(r => r.ExistsAsync("1")).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<BlacklistEntity>())).ReturnsAsync("1");

        // Act
        string? blacklist = await _facade.CreateOrUpdateAsync(model);

        // Assert
        Assert.Equal("1", blacklist);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<BlacklistEntity>()), Times.Once);
    }

    /// <summary>
    ///     Test method to verify that UpdateAsync returns null when the entity does not exist.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ReturnsNullForNonExistentEntity()
    {
        // Arrange
        BlacklistModel model = new() { Id = "2", DomainName = "nonexistent.com", Added = DateTime.Now };
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<BlacklistEntity>())).ReturnsAsync((string?)null);

        // Act
        string? blacklist = await _facade.UpdateAsync(model);

        // Assert
        Assert.Null(blacklist);
    }

    /// <summary>
    ///     Test method to ensure GetByIdAsync returns null when the entity does not exist.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenEntityDoesNotExist()
    {
        // Arrange
        string id = "nonexistent";
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((BlacklistEntity?)null);

        // Act
        BlacklistModel? blacklist = await _facade.GetByIdAsync(id);

        // Assert
        Assert.Null(blacklist);
    }

    /// <summary>
    ///     Test method to confirm that DeleteAsync completes successfully when an entry is removed.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_SuccessfulDeletion_ReturnsTaskCompleted()
    {
        // Arrange
        string id = "1";
        _repositoryMock.Setup(r => r.RemoveAsync(id)).Returns(Task.CompletedTask);

        // Act
        Task deleteOperation = _facade.DeleteAsync(id);

        // Assert
        Assert.True(deleteOperation.IsCompletedSuccessfully);
    }

    /// <summary>
    ///     Test method to verify CreateOrUpdateAsync creates a new entry when it does not exist.
    /// </summary>
    [Fact]
    public async Task CreateOrUpdateAsync_CreatesNewWhenNotExists()
    {
        // Arrange
        BlacklistModel model = new() { Id = "3", DomainName = "newsite.com", Added = DateTime.Now };
        _repositoryMock.Setup(r => r.ExistsAsync("3")).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.InsertAsync(It.IsAny<BlacklistEntity>())).ReturnsAsync("3");

        // Act
        string? blacklist = await _facade.CreateOrUpdateAsync(model);

        // Assert
        Assert.Equal("3", blacklist);
        _repositoryMock.Verify(r => r.InsertAsync(It.IsAny<BlacklistEntity>()), Times.Once);
    }

    /// <summary>
    ///     Test method to ensure GetAllAsync can return multiple models when multiple entries exist.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ReturnsMultipleModels()
    {
        // Arrange
        List<BlacklistEntity> entities =
            new() { new Mock<BlacklistEntity>().Object, new Mock<BlacklistEntity>().Object };
        List<BlacklistModel> models = new()
        {
            new BlacklistModel { Id = "1", DomainName = "example.com", Added = DateTime.Now },
            new BlacklistModel { Id = "2", DomainName = "test.com", Added = DateTime.Now }
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(entities);
        _mapperMock.Setup(m => m.Map<List<BlacklistModel>>(entities)).Returns(models);

        // Act
        List<BlacklistModel> blacklist = await _facade.GetAllAsync();

        // Assert
        Assert.Equal(2, blacklist.Count);
        Assert.Equal("1", blacklist[0].Id);
        Assert.Equal("2", blacklist[1].Id);
    }

    /// <summary>
    ///     Test method to verify that GetEntriesPerPageAsync returns an empty list when no entities are found.
    /// </summary>
    [Fact]
    public async Task GetEntriesPerPageAsync_ReturnsEmpty_WhenNoEntitiesFound()
    {
        // Arrange
        List<BlacklistEntity> emptyEntities = new();
        List<BlacklistModel> emptyModels = new();
        _repositoryMock.Setup(r => r.GetLimitOrGetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(emptyEntities);
        _mapperMock.Setup(m => m.Map<List<BlacklistModel>>(emptyEntities)).Returns(emptyModels);

        // Act
        List<BlacklistModel> blacklist = await _facade.GetEntriesPerPageAsync(2, 5);

        // Assert
        Assert.Empty(blacklist);
    }
}
