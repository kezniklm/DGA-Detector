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
public class BlacklistController(IBlacklistFacade blacklistFacade) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IList<BlacklistListModel>>> GetAll()
    {
        try
        {
            List<BlacklistListModel> blacklists = await blacklistFacade.GetAllAsync();
            return Ok(blacklists);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id:ObjectId}")]
    public async Task<ActionResult<BlacklistDetailModel>> Get(ObjectId id)
    {
        BlacklistDetailModel? blacklist = await blacklistFacade.GetByIdAsync(id);
        return blacklist is null ? NotFound() : blacklist;
    }

    [HttpPost]
    public async Task<ActionResult<ObjectId>> Create(BlacklistDetailModel blacklist) => await blacklistFacade.CreateAsync(blacklist);

    [HttpPatch]
    public async Task<ActionResult<ObjectId>> Update(BlacklistDetailModel blacklist)
    {
        ObjectId? updatedBlacklist = await blacklistFacade.CreateOrUpdateAsync(blacklist);
        return updatedBlacklist is null ? NotFound() : updatedBlacklist;
    }

    [HttpDelete]
    public async Task<ActionResult> Delete(ObjectId id)
    {
        try
        {
            await blacklistFacade.DeleteAsync(id);
        }
        catch (InvalidDeleteException ex)
        {
            return BadRequest();
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An unexpected error occurred.");
        }

        return Ok();
    }

    [HttpGet("{max:int}/{page:int}")]
    public async Task<ActionResult<IList<BlacklistListModel>>> GetWithPagination(int max, int page)
    {
        return await blacklistFacade.GetMaxOrGetAllAsync(max, page);
    }
}
