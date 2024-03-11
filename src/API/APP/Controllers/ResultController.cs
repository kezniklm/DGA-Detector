using BL.Facades.Interfaces;
using BL.Models.Result;
using Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace APP.Controllers;

[ApiController]
[Route("[controller]")]
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

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResultDetailModel>> Get(Guid id)
    {
        ResultDetailModel? result = await resultFacade.GetByIdAsync(id);
        return result is null ? NotFound() : result;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(ResultDetailModel result) => await resultFacade.CreateAsync(result);

    [HttpPatch]
    public async Task<ActionResult<Guid>> Update(ResultDetailModel result)
    {
        Guid? updatedResult = await resultFacade.CreateOrUpdateAsync(result);
        return updatedResult is null ? NotFound() : updatedResult;
    }

    [HttpDelete]
    public async Task<ActionResult> Delete(Guid id)
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
    public async Task<ActionResult<IList<ResultListModel>>> GetWithPagination(int max, int page)
    {
        return await resultFacade.GetMaxOrGetAllAsync(max, page);
    }
}
