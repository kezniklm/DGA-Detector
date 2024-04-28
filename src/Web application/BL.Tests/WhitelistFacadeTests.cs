/**
 * @file WhitelistFacadeTests.cs
 *
 * @brief Unit tests for the WhitelistFacade class.
 *
 * This file contains unit tests for the WhitelistFacade class. The WhitelistFacade class is responsible for managing operations related to whitelisted entities in the business logic layer.
 *
 * The main functionalities tested in this file include:
 * - Retrieving all entities successfully.
 * - Retrieving a single entity by ID.
 * - Creating a new entity.
 * - Updating an existing entity.
 * - Deleting an entity successfully.
 * - Retrieving paginated entities.
 * - Retrieving entities with a date filter.
 * - Getting the total count of entities.
 * - Handling scenarios where entities do not exist.
 * - Handling scenarios where entities cannot be created or updated.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-28
 * @copyright Copyright (c) 2024
 *
 */

using AutoMapper;
using BL.Facades;
using BL.Facades.Interfaces;
using BL.Models.Whitelist;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Moq;
using Xunit;

namespace BL.Tests;

/// <summary>
///     Unit tests for the WhitelistFacade class.
/// </summary>
public class WhitelistFacadeTests
{
    private readonly IWhitelistFacade _facade;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IWhitelistRepository> _repositoryMock;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WhitelistFacadeTests" /> class.
    /// </summary>
    public WhitelistFacadeTests()
    {
        _repositoryMock = new Mock<IWhitelistRepository>();
        _mapperMock = new Mock<IMapper>();
        _facade = new WhitelistFacade(_repositoryMock.Object, _mapperMock.Object);
    }

    /// <summary>
    ///     Test method to verify that GetAllAsync returns correct data.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ReturnsCorrectData()
    {
        // Arrange
        List<WhitelistEntity> entities = new() { new Mock<WhitelistEntity>().Object };
        List<WhitelistModel> models = new()
        {
            new WhitelistModel { Id = "1", DomainName = "example.com", Added = DateTime.Now }
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(entities);
        _mapperMock.Setup(m => m.Map<List<WhitelistModel>>(entities)).Returns(models);

        // Act
        List<WhitelistModel> whitelist = await _facade.GetAllAsync();

        // Assert
        Assert.Equal(models.Count, whitelist.Count);
        Assert.Equal(models[0].Id, whitelist[0].Id);
    }

    /// <summary>
    ///     Test method to ensure GetByIdAsync correctly returns a model if the entity exists.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ReturnsModel_IfExists()
    {
        // Arrange
        string id = "1";
        WhitelistEntity entity = new Mock<WhitelistEntity>().Object;
        WhitelistModel model = new() { Id = id, DomainName = "example.com", Added = DateTime.Now };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
        _mapperMock.Setup(m => m.Map<WhitelistModel>(entity)).Returns(model);

        // Act
        WhitelistModel? whitelist = await _facade.GetByIdAsync(id);

        // Assert
        Assert.NotNull(whitelist);
        Assert.Equal(id, whitelist.Id);
    }

    /// <summary>
    ///     Test method to verify that CreateAsync adds a new blacklist entry and returns the created ID.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ReturnsCreatedId()
    {
        // Arrange
        WhitelistModel model = new() { Id = "1", DomainName = "newsite.com", Added = DateTime.Now };
        WhitelistEntity entity = new Mock<WhitelistEntity>().Object;
        _mapperMock.Setup(m => m.Map<WhitelistEntity>(It.IsAny<WhitelistModel>())).Returns(entity);
        _repositoryMock.Setup(r => r.InsertAsync(entity)).ReturnsAsync("1");

        // Act
        string whitelist = await _facade.CreateAsync(model);

        // Assert
        Assert.Equal("1", whitelist);
    }

    /// <summary>
    ///     Test method to check that UpdateAsync updates an existing blacklist entry and returns the updated ID.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ReturnsUpdatedId()
    {
        // Arrange
        WhitelistModel model = new() { Id = "1", DomainName = "updatedsite.com", Added = DateTime.Now };
        WhitelistEntity entity = new Mock<WhitelistEntity>().Object;
        _mapperMock.Setup(m => m.Map<WhitelistEntity>(It.IsAny<WhitelistModel>())).Returns(entity);
        _repositoryMock.Setup(r => r.UpdateAsync(entity)).ReturnsAsync("1");

        // Act
        string? whitelist = await _facade.UpdateAsync(model);

        // Assert
        Assert.Equal("1", whitelist);
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
    public async Task GetEntriesPerPageAsync_ReturnsPaginatedWhitelists()
    {
        // Arrange
        List<WhitelistEntity> entities =
            new() { new Mock<WhitelistEntity>().Object, new Mock<WhitelistEntity>().Object };
        List<WhitelistModel> models = new()
        {
            new WhitelistModel { Id = "1", DomainName = "example.com", Added = DateTime.Now },
            new WhitelistModel { Id = "2", DomainName = "example.org", Added = DateTime.Now }
        };
        _repositoryMock.Setup(r => r.GetLimitOrGetAllAsync(0, 1, null)).ReturnsAsync(entities.Take(1));
        _mapperMock.Setup(m => m.Map<List<WhitelistModel>>(It.IsAny<IEnumerable<WhitelistEntity>>()))
            .Returns(models.Take(1).ToList());

        // Act
        List<WhitelistModel> whitelist = await _facade.GetEntriesPerPageAsync(1, 1);

        // Assert
        Assert.Single(whitelist);
        Assert.Equal("1", whitelist[0].Id);
    }

    /// <summary>
    ///     Test method to confirm that GetEntriesPerPageAsync correctly filters results by date when specified.
    /// </summary>
    [Fact]
    public async Task GetEntriesPerPageAsync_WithDateFilter_ReturnsFilteredWhitelists()
    {
        // Arrange
        DateTime startTime = DateTime.Now.AddDays(-1);
        DateTime endTime = DateTime.Now;
        List<WhitelistEntity> entities = new() { new Mock<WhitelistEntity>().Object };
        List<WhitelistModel> models = new()
        {
            new WhitelistModel { Id = "1", DomainName = "example.com", Added = DateTime.Now }
        };
        _repositoryMock.Setup(r => r.GetLimitOrGetAllAsync(0, 10, null)).ReturnsAsync(entities);
        _mapperMock.Setup(m => m.Map<List<WhitelistModel>>(It.IsAny<IEnumerable<WhitelistEntity>>())).Returns(models);

        // Act
        List<WhitelistModel> whitelist = await _facade.GetEntriesPerPageAsync(1, 10, null, startTime, endTime);

        // Assert
        Assert.Single(whitelist);
        Assert.Equal("1", whitelist[0].Id);
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
        WhitelistModel model = new() { Id = "1", DomainName = "updatedsite.com", Added = DateTime.Now };
        _repositoryMock.Setup(r => r.ExistsAsync("1")).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<WhitelistEntity>())).ReturnsAsync("1");

        // Act
        string? whitelist = await _facade.CreateOrUpdateAsync(model);

        // Assert
        Assert.Equal("1", whitelist);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<WhitelistEntity>()), Times.Once);
    }

    /// <summary>
    ///     Test method to verify that UpdateAsync returns null when the entity does not exist.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ReturnsNullForNonExistentEntity()
    {
        // Arrange
        WhitelistModel model = new() { Id = "2", DomainName = "nonexistent.com", Added = DateTime.Now };
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<WhitelistEntity>())).ReturnsAsync((string?)null);

        // Act
        string? whitelist = await _facade.UpdateAsync(model);

        // Assert
        Assert.Null(whitelist);
    }

    /// <summary>
    ///     Test method to ensure GetByIdAsync returns null when the entity does not exist.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenEntityDoesNotExist()
    {
        // Arrange
        string id = "nonexistent";
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((WhitelistEntity?)null);

        // Act
        WhitelistModel? whitelist = await _facade.GetByIdAsync(id);

        // Assert
        Assert.Null(whitelist);
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
        WhitelistModel model = new() { Id = "3", DomainName = "newsite.com", Added = DateTime.Now };
        _repositoryMock.Setup(r => r.ExistsAsync("3")).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.InsertAsync(It.IsAny<WhitelistEntity>())).ReturnsAsync("3");

        // Act
        string? whitelist = await _facade.CreateOrUpdateAsync(model);

        // Assert
        Assert.Equal("3", whitelist);
        _repositoryMock.Verify(r => r.InsertAsync(It.IsAny<WhitelistEntity>()), Times.Once);
    }

    /// <summary>
    ///     Test method to ensure GetAllAsync can return multiple models when multiple entries exist.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ReturnsMultipleModels()
    {
        // Arrange
        List<WhitelistEntity> entities =
            new() { new Mock<WhitelistEntity>().Object, new Mock<WhitelistEntity>().Object };
        List<WhitelistModel> models = new()
        {
            new WhitelistModel { Id = "1", DomainName = "example.com", Added = DateTime.Now },
            new WhitelistModel { Id = "2", DomainName = "test.com", Added = DateTime.Now }
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(entities);
        _mapperMock.Setup(m => m.Map<List<WhitelistModel>>(entities)).Returns(models);

        // Act
        List<WhitelistModel> whitelist = await _facade.GetAllAsync();

        // Assert
        Assert.Equal(2, whitelist.Count);
        Assert.Equal("1", whitelist[0].Id);
        Assert.Equal("2", whitelist[1].Id);
    }

    /// <summary>
    ///     Test method to verify that GetEntriesPerPageAsync returns an empty list when no entities are found.
    /// </summary>
    [Fact]
    public async Task GetEntriesPerPageAsync_ReturnsEmpty_WhenNoEntitiesFound()
    {
        // Arrange
        List<WhitelistEntity> emptyEntities = new();
        List<WhitelistModel> emptyModels = new();
        _repositoryMock.Setup(r => r.GetLimitOrGetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(emptyEntities);
        _mapperMock.Setup(m => m.Map<List<WhitelistModel>>(emptyEntities)).Returns(emptyModels);

        // Act
        List<WhitelistModel> whitelist = await _facade.GetEntriesPerPageAsync(2, 5);

        // Assert
        Assert.Empty(whitelist);
    }
}
