using BL.Facades.Interfaces;
using BL.Models.Whitelist;
using Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace APP.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class WhitelistController(IWhitelistFacade whitelistFacade, ILogger<WhitelistController> logger)
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

    [HttpGet("{id:ObjectId}")]
    public async Task<ActionResult<WhitelistModel>> Get(ObjectId id)
    {
        try
        {
            WhitelistModel? whitelist = await whitelistFacade.GetByIdAsync(id);
            if (whitelist == null)
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
    public async Task<ActionResult<ObjectId>> Create(WhitelistModel whitelist)
    {
        try
        {
            ObjectId createdWhitelist = await whitelistFacade.CreateAsync(whitelist);
            return Ok(createdWhitelist);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating whitelist.");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPatch]
    public async Task<ActionResult<ObjectId>> Update(WhitelistModel whitelist)
    {
        try
        {
            ObjectId? updatedWhitelist = await whitelistFacade.CreateOrUpdateAsync(whitelist);
            if (updatedWhitelist == null)
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

    [HttpDelete("{id:ObjectId}")]
    public async Task<ActionResult> Delete(ObjectId id)
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
    public async Task<ActionResult<IList<WhitelistModel>>> GetWithPaginationAndFilter(int max, int page, string? filter)
    {
        try
        {
            List<WhitelistModel> results = await whitelistFacade.GetEntriesPerPageAsync(max, page, filter);
            return Ok(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting whitelists with pagination.");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("count")]
    public async Task<ActionResult<long>> GetTotalCount()
    {
        try
        {
            long count = await whitelistFacade.GetNumberOfAllAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting total count of whitelists.");
            return StatusCode(500, "Internal server error");
        }
    }
}
