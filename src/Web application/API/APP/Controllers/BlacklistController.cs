/**
 * @file BlacklistController.cs
 *
 * @brief Provides endpoints for managing blacklist entries.
 *
 * This file contains the implementation of the BlacklistController class, which provides HTTP endpoints for managing blacklist entries. It handles CRUD operations for blacklisted items, including fetching, creating, updating, and deleting entries. Additionally, it supports pagination and filtering of blacklist entries.
 *
 * The main functionalities of this controller include:
 * - Retrieving all blacklist entries.
 * - Retrieving a blacklist entry by its ID.
 * - Creating a new blacklist entry.
 * - Moving a result to the blacklist.
 * - Updating an existing blacklist entry.
 * - Deleting a blacklist entry.
 * - Retrieving blacklist entries with pagination and filtering.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using BL.Facades.Interfaces;
using BL.Models.Blacklist;
using BL.Models.Result;
using Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APP.Controllers;

/// <summary>
///     Controller for managing blacklist operations.
/// </summary>
[ApiController]
[Route("[controller]")]
[Authorize]
public class BlacklistController(
    IBlacklistFacade blacklistFacade,
    IResultFacade resultFacade,
    ILogger<BlacklistController> logger)
    : ControllerBase
{
    /// <summary>
    ///     Gets all blacklist entries.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IList<BlacklistModel>>> GetAll()
    {
        try
        {
            logger.LogInformation("Fetching all blacklist entries.");
            List<BlacklistModel> blacklists = await blacklistFacade.GetAllAsync();
            return Ok(blacklists);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching all blacklist entries.");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Gets a specific blacklist entry by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<BlacklistModel>> Get(string id)
    {
        try
        {
            logger.LogInformation("Fetching blacklist entry with ID: {Id}", id);
            BlacklistModel? blacklist = await blacklistFacade.GetByIdAsync(id);
            if (blacklist is null)
            {
                logger.LogWarning("Blacklist entry not found for ID: {Id}", id);
                return NotFound();
            }

            return Ok(blacklist);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching blacklist entry with ID: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Creates a new blacklist entry.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<string>> Create(BlacklistModel blacklist)
    {
        try
        {
            logger.LogInformation("Creating a new blacklist entry.");
            string resultId = await blacklistFacade.CreateAsync(blacklist);
            return Ok(resultId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating a new blacklist entry.");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Moves a result to a new blacklist entry.
    /// </summary>
    [HttpPost("MoveResultToBlacklist")]
    public async Task<ActionResult<string>> MoveResultToBlacklist(ResultModel resultModel)
    {
        try
        {
            logger.LogInformation("Moving result to a new blacklist entry.");
            string resultId = await blacklistFacade.MoveResultToBlacklist(resultModel);

            await resultFacade.DeleteAsync(resultId);
            return Ok(resultId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while moving result to a new blacklist entry.");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Updates a blacklist entry.
    /// </summary>
    [HttpPatch]
    public async Task<ActionResult<string>> Update([FromBody] BlacklistModel blacklist)
    {
        try
        {
            logger.LogInformation("Updating blacklist entry with ID: {Id}", blacklist.Id);
            string? updatedBlacklist = await blacklistFacade.CreateOrUpdateAsync(blacklist);
            if (updatedBlacklist is null)
            {
                logger.LogWarning("Blacklist entry for update not found with ID: {Id}", blacklist.Id);
                return NotFound();
            }

            return Ok(updatedBlacklist);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating blacklist entry with ID: {Id}", blacklist?.Id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Deletes a blacklist entry by ID.
    /// </summary>
    /// <param name="id">The ID of the blacklist entry to delete.</param>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            logger.LogInformation("Deleting blacklist entry with ID: {Id}", id);
            await blacklistFacade.DeleteAsync(id);
            return Ok();
        }
        catch (InvalidDeleteException ex)
        {
            logger.LogWarning(ex, "Invalid delete operation for blacklist entry with ID: {Id}", id);
            return BadRequest();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting blacklist entry with ID: {Id}", id);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    /// <summary>
    ///     Gets blacklist entries with pagination and optional filtering.
    /// </summary>
    /// <param name="max">Maximum number of entries per page.</param>
    /// <param name="page">Page number.</param>
    /// <param name="filter">Optional filter string.</param>
    /// <param name="startDate">Optional start date for filtering.</param>
    /// <param name="endDate">End date for filtering.</param>
    [HttpGet("{max:int}/{page:int}/{filter?}")]
    public async Task<ActionResult<IList<BlacklistModel>>> GetWithPaginationAndFilter(int max, int page, string? filter,
        DateTime? startDate, DateTime endDate)
    {
        try
        {
            logger.LogInformation(
                "Fetching blacklist entries with pagination. Max: {Max}, Page: {Page}, Filter: {Filter}", max, page,
                filter);
            List<BlacklistModel> entries =
                await blacklistFacade.GetEntriesPerPageAsync(page, max, filter, startDate, endDate);
            return Ok(entries);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching blacklist entries with pagination.");
            return StatusCode(500, "Internal server error");
        }
    }
}
