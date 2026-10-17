using Microsoft.AspNetCore.Mvc;
using TasksAPI.Dtos;
using TasksAPI.Services;

namespace TasksAPI.Controllers;

[ApiController]
[Route("api/categories")]
[Produces("application/json")]
public class CategoryController (CategoryService categoryService) : ControllerBase
{
    private readonly CategoryService categoryService = categoryService;


    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> List()
    {
        IEnumerable<CategoryResponse> result = await categoryService.ListAsync();

        return Ok(result);
    }


    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponse>> GetById(int id)
    {
        CategoryResponse? result = await categoryService.GetByIdAsync(id);

        if (result is null)
        {
            return NotFound();
        }

        return result;
    }


    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryResponse>> AddAsync([FromBody] CategoryRequest request)
    {
        CategoryResponse result = await categoryService.AddAsync(request);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }


    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponse>> UpdateAsync(int id, [FromBody] CategoryRequest request)
    {
        CategoryResponse? result = await categoryService.UpdateAsync(id, request);

        if (result is null)
        {
            return NotFound();
        }

        return result;
    }


    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        bool deleted = await categoryService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
