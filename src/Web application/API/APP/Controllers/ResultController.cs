using BL.Facades.Interfaces;
using BL.Models.Result;
using Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace APP.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ResultController(IResultFacade resultFacade) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IList<ResultListModel>>> GetAll()
    {
        try
        {
            List<ResultListModel> results = await resultFacade.GetAllAsync();
            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id:ObjectId}")]
    public async Task<ActionResult<ResultDetailModel>> Get(ObjectId id)
    {
        ResultDetailModel? result = await resultFacade.GetByIdAsync(id);
        return result is null ? NotFound() : result;
    }

    [HttpPost]
    public async Task<ActionResult<ObjectId>> Create(ResultDetailModel result) =>
        await resultFacade.CreateAsync(result);

    [HttpPatch]
    public async Task<ActionResult<ObjectId>> Update(ResultDetailModel result)
    {
        ObjectId? updatedResult = await resultFacade.CreateOrUpdateAsync(result);
        return updatedResult is null ? NotFound() : updatedResult;
    }

    [HttpDelete]
    public async Task<ActionResult> Delete(ObjectId id)
    {
        try
        {
            await resultFacade.DeleteAsync(id);
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
    public async Task<ActionResult<IList<ResultListModel>>> GetWithPagination(int max, int page) =>
        await resultFacade.GetMaxOrGetAllAsync(max, page);
}
