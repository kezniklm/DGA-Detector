using BL.Facades.Interfaces;
using BL.Models.Result;
using Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APP.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ResultController(IResultFacade resultFacade, ILogger<ResultController> logger)
    : ControllerBase
{
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

    [HttpGet("{id}")]
    public async Task<ActionResult<ResultModel>> Get(string id)
    {
        try
        {
            ResultModel? result = await resultFacade.GetByIdAsync(id);
            if (result == null)
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

    [HttpPatch]
    public async Task<ActionResult<string>> Update(ResultModel result)
    {
        try
        {
            string? updatedResult = await resultFacade.CreateOrUpdateAsync(result);
            if (updatedResult == null)
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

    [HttpGet("{max:int}/{page:int}/{filter?}")]
    public async Task<ActionResult<IList<ResultModel>>> GetWithPaginationAndFilter(int max, int page, string? filter)
    {
        try
        {
            List<ResultModel> results = await resultFacade.GetEntriesPerPageAsync(max, page, filter);
            return Ok(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting results with pagination.");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("NumberOfDomainsToday")]
    public async Task<ActionResult<long>> NumberOfDomainsToday()
    {
        try
        {
            long count = await resultFacade.GetNumberOfDomainsTodayAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting number of domains today.");
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [HttpGet("PositiveResultsToday")]
    public async Task<ActionResult<long>> PositiveResultsToday()
    {
        try
        {
            long count = await resultFacade.GetPositiveDetectionResultsTodayAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting positive results today.");
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [HttpGet("FilteredByBlacklist")]
    public async Task<ActionResult<long>> FilteredByBlacklist()
    {
        try
        {
            long count = await resultFacade.GetFilteredByBlacklistCountAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting filtered by blacklist count.");
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [HttpGet("count")]
    public async Task<ActionResult<long>> GetTotalCount()
    {
        try
        {
            long count = await resultFacade.GetNumberOfAllAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting total count.");
            return StatusCode(500, "Internal server error");
        }
    }
}
