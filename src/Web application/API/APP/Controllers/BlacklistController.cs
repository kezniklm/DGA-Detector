using APP.DTOs;
using BL.Facades.Interfaces;
using BL.Models.Blacklist;
using Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace APP.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class BlacklistController(IBlacklistFacade blacklistFacade, ILogger<BlacklistController> logger)
    : ControllerBase
{
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

    [HttpGet("{id:ObjectId}")]
    public async Task<ActionResult<BlacklistModel>> Get(ObjectId id)
    {
        try
        {
            logger.LogInformation("Fetching blacklist entry with ID: {Id}", id);
            BlacklistModel? blacklist = await blacklistFacade.GetByIdAsync(id);
            if (blacklist == null)
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

    [HttpPost]
    public async Task<ActionResult<ObjectId>> Create(BlacklistModel blacklist)
    {
        try
        {
            logger.LogInformation("Creating a new blacklist entry.");
            ObjectId resultId = await blacklistFacade.CreateAsync(blacklist);
            return Ok(resultId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating a new blacklist entry.");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPatch]
    public async Task<ActionResult<ObjectId>> Update([FromBody] BlacklistDto blacklistDto)
    {
        BlacklistModel? blacklist = null;
        try
        {
            blacklist = BlacklistModelDeserializer.DeserializeBlacklistModel(blacklistDto);
            logger.LogInformation("Updating blacklist entry with ID: {Id}", blacklist.Id);
            ObjectId? updatedBlacklist = await blacklistFacade.CreateOrUpdateAsync(blacklist);
            if (updatedBlacklist == null)
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

    [HttpDelete("{id:ObjectId}")]
    public async Task<ActionResult> Delete(ObjectId id)
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

    [HttpGet("{max:int}/{page:int}/{filter?}")]
    public async Task<ActionResult<IList<BlacklistModel>>> GetWithPaginationAndFilter(int max, int page, string? filter)
    {
        try
        {
            logger.LogInformation(
                "Fetching blacklist entries with pagination. Max: {Max}, Page: {Page}, Filter: {Filter}", max, page,
                filter);
            List<BlacklistModel> entries = await blacklistFacade.GetEntriesPerPageAsync(max, page, filter);
            return Ok(entries);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching blacklist entries with pagination.");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("count")]
    public async Task<ActionResult<long>> GetTotalCount()
    {
        try
        {
            logger.LogInformation("Fetching total count of blacklist entries.");
            long count = await blacklistFacade.GetNumberOfAllAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching total count of blacklist entries.");
            return StatusCode(500, "Internal server error");
        }
    }
}
