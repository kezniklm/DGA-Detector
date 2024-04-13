using BL.Facades.Interfaces;
using BL.Models.Result;
using BL.Models.Whitelist;
using Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APP.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class WhitelistController(
    IWhitelistFacade whitelistFacade,
    IResultFacade resultFacade,
    ILogger<WhitelistController> logger)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IList<WhitelistModel>>> GetAll()
    {
        try
        {
            List<WhitelistModel> whitelists = await whitelistFacade.GetAllAsync();
            return Ok(whitelists);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all whitelists.");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WhitelistModel>> Get(string id)
    {
        try
        {
            WhitelistModel? whitelist = await whitelistFacade.GetByIdAsync(id);
            if (whitelist is null)
            {
                logger.LogWarning("Whitelist not found for ID: {Id}", id);
                return NotFound();
            }

            return whitelist;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting whitelist with ID: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<string>> Create(WhitelistModel whitelist)
    {
        try
        {
            string createdWhitelist = await whitelistFacade.CreateAsync(whitelist);
            return Ok(createdWhitelist);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating whitelist.");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("MoveResultToWhitelist")]
    public async Task<ActionResult<string>> MoveResultToWhitelist(ResultModel resultModel)
    {
        try
        {
            logger.LogInformation("Moving result to a new blacklist entry.");
            string resultId = await whitelistFacade.MoveResultToWhitelist(resultModel);

            await resultFacade.DeleteAsync(resultId);
            return Ok(resultId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while moving result to a new blacklist entry.");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPatch]
    public async Task<ActionResult<string>> Update(WhitelistModel whitelist)
    {
        try
        {
            string? updatedWhitelist = await whitelistFacade.CreateOrUpdateAsync(whitelist);
            if (updatedWhitelist is null)
            {
                logger.LogWarning("Whitelist not found for update.");
                return NotFound();
            }

            return Ok(updatedWhitelist);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating whitelist.");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            await whitelistFacade.DeleteAsync(id);
            return Ok();
        }
        catch (InvalidDeleteException ex)
        {
            logger.LogWarning(ex, "Invalid delete attempt for ID: {Id}", id);
            return BadRequest();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting whitelist with ID: {Id}", id);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpGet("{max:int}/{page:int}/{filter?}")]
    public async Task<ActionResult<IList<WhitelistModel>>> GetWithPaginationAndFilter(int max, int page, string? filter,
        DateTime? startDate, DateTime endDate)
    {
        try
        {
            List<WhitelistModel> results =
                await whitelistFacade.GetEntriesPerPageAsync(max, page, filter, startDate, endDate);
            return Ok(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting whitelists with pagination.");
            return StatusCode(500, "Internal server error");
        }
    }
}
