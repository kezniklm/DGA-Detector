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
public class WhitelistController(IWhitelistFacade whitelistFacade) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IList<WhitelistListModel>>> GetAll()
    {
        try
        {
            List<WhitelistListModel> whitelists = await whitelistFacade.GetAllAsync();
            return Ok(whitelists);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id:ObjectId}")]
    public async Task<ActionResult<WhitelistDetailModel>> Get(ObjectId id)
    {
        WhitelistDetailModel? whitelist = await whitelistFacade.GetByIdAsync(id);
        return whitelist is null ? NotFound() : whitelist;
    }

    [HttpPost]
    public async Task<ActionResult<ObjectId>> Create(WhitelistDetailModel whitelist) =>
        await whitelistFacade.CreateAsync(whitelist);

    [HttpPatch]
    public async Task<ActionResult<ObjectId>> Update(WhitelistDetailModel whitelist)
    {
        ObjectId? updatedWhitelist = await whitelistFacade.CreateOrUpdateAsync(whitelist);
        return updatedWhitelist is null ? NotFound() : updatedWhitelist;
    }

    [HttpDelete]
    public async Task<ActionResult> Delete(ObjectId id)
    {
        try
        {
            await whitelistFacade.DeleteAsync(id);
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
    public async Task<ActionResult<IList<WhitelistListModel>>> GetWithPagination(int max, int page) =>
        await whitelistFacade.GetMaxOrGetAllAsync(max, page);
}
