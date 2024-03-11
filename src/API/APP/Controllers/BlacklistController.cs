using BL.Facades.Interfaces;
using BL.Models.Blacklist;
using Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace APP.Controllers;

[ApiController]
[Route("[controller]")]
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

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BlacklistDetailModel>> Get(Guid id)
    {
        BlacklistDetailModel? blacklist = await blacklistFacade.GetByIdAsync(id);
        return blacklist is null ? NotFound() : blacklist;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(BlacklistDetailModel blacklist) => await blacklistFacade.CreateAsync(blacklist);

    [HttpPatch]
    public async Task<ActionResult<Guid>> Update(BlacklistDetailModel blacklist)
    {
        Guid? updatedBlacklist = await blacklistFacade.CreateOrUpdateAsync(blacklist);
        return updatedBlacklist is null ? NotFound() : updatedBlacklist;
    }

    [HttpDelete]
    public async Task<ActionResult> Delete(Guid id)
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
