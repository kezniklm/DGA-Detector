/**
 * @file ResultController.cs
 *
 * @brief Controller for managing results in the application.
 *
 * This controller handles various operations related to results, including retrieving, creating, updating, and deleting results. It interacts with the IResultFacade interface for performing business logic operations and logging errors using ILogger<ResultController>.
 *
 * The main functionalities of this controller include:
 * - Retrieving all results.
 * - Retrieving a specific result by ID.
 * - Creating a new result.
 * - Updating an existing result.
 * - Deleting a result by ID.
 * - Retrieving results with pagination and optional filtering.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using BL.Facades.Interfaces;
using BL.Models.Result;
using Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APP.Controllers;

/// <summary>
///     Controller for managing results.
/// </summary>
[ApiController]
[Route("[controller]")]
[Authorize]
public class ResultController(IResultFacade resultFacade, ILogger<ResultController> logger)
    : ControllerBase
{
    /// <summary>
    ///     Retrieves all results.
    /// </summary>
    /// <returns>List of ResultModel.</returns>
    [HttpGet]
    public async Task<ActionResult<IList<ResultModel>>> GetAll()
    {
        try
        {
            List<ResultModel> results = await resultFacade.GetAllAsync();
            return Ok(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all results.");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Retrieves a result by ID.
    /// </summary>
    /// <param name="id">The ID of the result.</param>
    /// <returns>ResultModel.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ResultModel>> Get(string id)
    {
        try
        {
            ResultModel? result = await resultFacade.GetByIdAsync(id);
            if (result is null)
            {
                logger.LogWarning("Result not found for ID: {Id}", id);
                return NotFound();
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting result with ID: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Creates a new result.
    /// </summary>
    /// <param name="result">The result to create.</param>
    /// <returns>String indicating the created result.</returns>
    [HttpPost]
    public async Task<ActionResult<string>> Create(ResultModel result)
    {
        try
        {
            string createdResult = await resultFacade.CreateAsync(result);
            return Ok(createdResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating result.");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Updates an existing result.
    /// </summary>
    /// <param name="result">The result to update.</param>
    /// <returns>String indicating the updated result.</returns>
    [HttpPatch]
    public async Task<ActionResult<string>> Update(ResultModel result)
    {
        try
        {
            string? updatedResult = await resultFacade.CreateOrUpdateAsync(result);
            if (updatedResult is null)
            {
                logger.LogWarning("Result not found for update.");
                return NotFound();
            }

            return Ok(updatedResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating result.");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Deletes a result by ID.
    /// </summary>
    /// <param name="id">The ID of the result to delete.</param>
    /// <returns>ActionResult indicating success or failure.</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            await resultFacade.DeleteAsync(id);
            return Ok();
        }
        catch (InvalidDeleteException ex)
        {
            logger.LogWarning(ex, "Invalid delete attempt for ID: {Id}", id);
            return BadRequest();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting result with ID: {Id}", id);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    /// <summary>
    ///     Retrieves results with pagination and optional filtering.
    /// </summary>
    /// <param name="max">Maximum number of results per page.</param>
    /// <param name="page">Page number.</param>
    /// <param name="filter">Optional filter string.</param>
    /// <param name="startDate">Optional start date for filtering.</param>
    /// <param name="endDate">End date for filtering.</param>
    /// <returns>List of ResultModel.</returns>
    [HttpGet("{max:int}/{page:int}/{filter?}")]
    public async Task<ActionResult<IList<ResultModel>>> GetWithPaginationAndFilter(int max, int page, string? filter,
        DateTime? startDate, DateTime endDate)
    {
        try
        {
            List<ResultModel> results =
                await resultFacade.GetEntriesPerPageAsync(page, max, filter, startDate, endDate);
            return Ok(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting results with pagination.");
            return StatusCode(500, "Internal server error");
        }
    }
}
