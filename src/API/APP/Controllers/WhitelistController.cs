using BL.Facades.Interfaces;
using BL.Models.Whitelist;
using Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace APP.Controllers;

[ApiController]
[Route("[controller]")]
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

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WhitelistDetailModel>> Get(Guid id)
    {
        WhitelistDetailModel? whitelist = await whitelistFacade.GetByIdAsync(id);
        return whitelist is null ? NotFound() : whitelist;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(WhitelistDetailModel whitelist) => await whitelistFacade.CreateAsync(whitelist);

    [HttpPatch]
    public async Task<ActionResult<Guid>> Update(WhitelistDetailModel whitelist)
    {
        Guid? updatedWhitelist = await whitelistFacade.CreateOrUpdateAsync(whitelist);
        return updatedWhitelist is null ? NotFound() : updatedWhitelist;
    }

    [HttpDelete]
    public async Task<ActionResult> Delete(Guid id)
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
    public async Task<ActionResult<IList<WhitelistListModel>>> GetWithPagination(int max, int page)
    {
        return await whitelistFacade.GetMaxOrGetAllAsync(max, page);
    }
}
