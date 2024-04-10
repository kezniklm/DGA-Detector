using BL.Facades.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace APP.Controllers;

[ApiController]
[Route("[controller]")]
public class PublicController(
    IBlacklistFacade blacklistFacade,
    IWhitelistFacade whitelistFacade,
    IResultFacade resultFacade,
    ILogger<PublicController> logger)
    : ControllerBase
{
    [HttpGet("blacklist/count")]
    public async Task<ActionResult<long>> GetTotalCountOfBlacklist()
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

    [HttpGet("whitelist/count")]
    public async Task<ActionResult<long>> GetTotalCountOfWhiteList()
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

    [HttpGet("results/count")]
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
}
