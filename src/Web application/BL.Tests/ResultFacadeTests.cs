/**
 * @file ResultFacadeTests.cs
 *
 * @brief Unit tests for the ResultFacade class.
 *
 * This file contains unit tests for the ResultFacade class, which is part of the Business Logic Layer and interacts with the data layer and data transformation processes to execute operations related to results management.
 *
 * The main functionalities tested in this file include:
 * - Retrieving all results correctly.
 * - Retrieving a result by its ID.
 * - Creating a new result and returning its ID.
 * - Updating an existing result and verifying the returned ID.
 * - Deleting a result by its ID.
 * - Handling pagination and filters in result retrieval.
 * - Counting all entries in the repository.
 * - Creating or updating an existing entry based on existence checks.
 * - Managing empty and non-existing scenarios appropriately in various methods.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using AutoMapper;
using BL.Facades;
using BL.Models.Result;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Moq;
using Xunit;

namespace BL.Tests;

/// <summary>
///     Unit tests for the ResultFacade class, providing a comprehensive suite of tests for result operations.
/// </summary>
public class ResultFacadeTests
{
    private readonly ResultFacade _facade;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IResultRepository> _repositoryMock;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ResultFacadeTests" /> class.
    ///     Sets up the mocks for dependencies and the facade instance.
    /// </summary>
    public ResultFacadeTests()
    {
        _repositoryMock = new Mock<IResultRepository>();
        _mapperMock = new Mock<IMapper>();
        _facade = new ResultFacade(_repositoryMock.Object, _mapperMock.Object);
    }

    /// <summary>
    ///     Test method to verify that GetAllAsync retrieves all result data correctly and maps it to the expected model list.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ReturnsCorrectData()
    {
        // Arrange
        List<ResultEntity> entities = new() { new Mock<ResultEntity>().Object };
        List<ResultModel> models = new()
        {
            new ResultModel { Id = "1", DomainName = "example.com", Detected = DateTime.Now }
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(entities);
        _mapperMock.Setup(m => m.Map<List<ResultModel>>(entities)).Returns(models);

        // Act
        List<ResultModel> result = await _facade.GetAllAsync();

        // Assert
        Assert.Equal(models.Count, result.Count);
        Assert.Equal(models[0].Id, result[0].Id);
    }

    /// <summary>
    ///     Test method to ensure that GetByIdAsync returns a model if the result exists in the repository.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ReturnsModel_IfExists()
    {
        // Arrange
        string id = "1";
        ResultEntity entity = new Mock<ResultEntity>().Object;
        ResultModel model = new() { Id = id, DomainName = "example.com" };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
        _mapperMock.Setup(m => m.Map<ResultModel>(entity)).Returns(model);

        // Act
        ResultModel? result = await _facade.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
    }

    /// <summary>
    ///     Test method to confirm that CreateAsync correctly inserts a new result and returns its ID.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ReturnsCreatedId()
    {
        // Arrange
        ResultModel model = new() { Id = "1", DomainName = "newsite.com" };
        ResultEntity entity = new Mock<ResultEntity>().Object;
        _mapperMock.Setup(m => m.Map<ResultEntity>(It.IsAny<ResultModel>())).Returns(entity);
        _repositoryMock.Setup(r => r.InsertAsync(entity)).ReturnsAsync("1");

        // Act
        string result = await _facade.CreateAsync(model);

        // Assert
        Assert.Equal("1", result);
    }

    /// <summary>
    ///     Test method to verify that UpdateAsync updates a result and returns the updated ID.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ReturnsUpdatedId()
    {
        // Arrange
        ResultModel model = new() { Id = "1", DomainName = "updatedsite.com" };
        ResultEntity entity = new Mock<ResultEntity>().Object;
        _mapperMock.Setup(m => m.Map<ResultEntity>(It.IsAny<ResultModel>())).Returns(entity);
        _repositoryMock.Setup(r => r.UpdateAsync(entity)).ReturnsAsync("1");

        // Act
        string? result = await _facade.UpdateAsync(model);

        // Assert
        Assert.Equal("1", result);
    }

    /// <summary>
    ///     Test method to check that DeleteAsync calls the repository's Remove method once.
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
    ///     Test method to validate that pagination and optional filtering work as expected in GetEntriesPerPageAsync.
    /// </summary>
    [Fact]
    public async Task GetEntriesPerPageAsync_ReturnsPaginatedResults()
    {
        // Arrange
        List<ResultEntity> entities = new() { new Mock<ResultEntity>().Object, new Mock<ResultEntity>().Object };
        List<ResultModel> models = new()
        {
            new ResultModel { Id = "1", DomainName = "example.com" },
            new ResultModel { Id = "2", DomainName = "example.org" }
        };
        _repositoryMock.Setup(r => r.GetLimitOrGetAllAsync(0, 1, null)).ReturnsAsync(entities.Take(1));
        _mapperMock.Setup(m => m.Map<List<ResultModel>>(It.IsAny<IEnumerable<ResultEntity>>()))
            .Returns(models.Take(1).ToList());

        // Act
        List<ResultModel> result = await _facade.GetEntriesPerPageAsync(1, 1);

        // Assert
        Assert.Single(result);
        Assert.Equal("1", result[0].Id);
    }

    /// <summary>
    ///     Test method to verify that the GetEntriesPerPageAsync method returns correct results when filtering by date range.
    /// </summary>
    [Fact]
    public async Task GetEntriesPerPageAsync_WithDateFilter_ReturnsFilteredResults()
    {
        // Arrange
        DateTime startTime = DateTime.Now.AddDays(-1);
        DateTime endTime = DateTime.Now;
        List<ResultEntity> entities = new() { new Mock<ResultEntity>().Object };
        List<ResultModel> models = new()
        {
            new ResultModel { Id = "1", DomainName = "example.com", Detected = DateTime.Now }
        };
        _repositoryMock.Setup(r => r.GetLimitOrGetAllAsync(0, 10, null)).ReturnsAsync(entities);
        _mapperMock.Setup(m => m.Map<List<ResultModel>>(It.IsAny<IEnumerable<ResultEntity>>())).Returns(models);

        // Act
        List<ResultModel> result = await _facade.GetEntriesPerPageAsync(1, 10, null, startTime, endTime);

        // Assert
        Assert.Single(result);
        Assert.Equal("1", result[0].Id);
    }

    /// <summary>
    ///     Test method to verify that GetNumberOfAllAsync correctly counts all entries in the repository.
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
    ///     Test method to ensure that CreateOrUpdateAsync updates an existing result when it exists, or creates a new one if
    ///     it does not.
    /// </summary>
    [Fact]
    public async Task CreateOrUpdateAsync_UpdatesWhenExists()
    {
        // Arrange
        ResultModel model = new() { Id = "1", DomainName = "updatedsite.com" };
        _repositoryMock.Setup(r => r.ExistsAsync("1")).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ResultEntity>())).ReturnsAsync("1");

        // Act
        string? result = await _facade.CreateOrUpdateAsync(model);

        // Assert
        Assert.Equal("1", result);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ResultEntity>()), Times.Once);
    }

    /// <summary>
    ///     Test method to ensure that the UpdateAsync method returns null when the entity to be updated does not exist in the
    ///     database.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ReturnsNullForNonExistentEntity()
    {
        // Arrange
        ResultModel model = new() { Id = "2", DomainName = "nonexistent.com" };
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ResultEntity>())).ReturnsAsync((string?)null);

        // Act
        string? result = await _facade.UpdateAsync(model);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    ///     Test method to confirm that GetByIdAsync returns null when the specified ID does not correspond to any existing
    ///     entity.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenEntityDoesNotExist()
    {
        // Arrange
        string id = "nonexistent";
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ResultEntity?)null);

        // Act
        ResultModel? result = await _facade.GetByIdAsync(id);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    ///     Test method to ensure that the DeleteAsync method successfully completes the deletion task.
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
    ///     Test method to ensure that CreateOrUpdateAsync creates a new entry when the specified ID does not exist in the
    ///     database.
    /// </summary>
    [Fact]
    public async Task CreateOrUpdateAsync_CreatesNewWhenNotExists()
    {
        // Arrange
        ResultModel model = new() { Id = "3", DomainName = "newsite.com" };
        _repositoryMock.Setup(r => r.ExistsAsync("3")).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.InsertAsync(It.IsAny<ResultEntity>())).ReturnsAsync("3");

        // Act
        string? result = await _facade.CreateOrUpdateAsync(model);

        // Assert
        Assert.Equal("3", result);
        _repositoryMock.Verify(r => r.InsertAsync(It.IsAny<ResultEntity>()), Times.Once);
    }

    /// <summary>
    ///     Test method to confirm that GetAllAsync correctly handles scenarios where multiple models are returned.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ReturnsMultipleModels()
    {
        // Arrange
        List<ResultEntity> entities = new() { new Mock<ResultEntity>().Object, new Mock<ResultEntity>().Object };
        List<ResultModel> models = new()
        {
            new ResultModel { Id = "1", DomainName = "example.com" },
            new ResultModel { Id = "2", DomainName = "test.com" }
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(entities);
        _mapperMock.Setup(m => m.Map<List<ResultModel>>(entities)).Returns(models);

        // Act
        List<ResultModel> result = await _facade.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("1", result[0].Id);
        Assert.Equal("2", result[1].Id);
    }

    /// <summary>
    ///     Test method to ensure that GetEntriesPerPageAsync returns an empty list when no entities are found.
    /// </summary>
    [Fact]
    public async Task GetEntriesPerPageAsync_ReturnsEmpty_WhenNoEntitiesFound()
    {
        // Arrange
        List<ResultEntity> emptyEntities = new();
        List<ResultModel> emptyModels = new();
        _repositoryMock.Setup(r => r.GetLimitOrGetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(emptyEntities);
        _mapperMock.Setup(m => m.Map<List<ResultModel>>(emptyEntities)).Returns(emptyModels);

        // Act
        List<ResultModel> result = await _facade.GetEntriesPerPageAsync(2, 5);

        // Assert
        Assert.Empty(result);
    }
}
