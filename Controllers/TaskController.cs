using Microsoft.AspNetCore.Mvc;
using TasksAPI.Dtos;
using TasksAPI.Services;

namespace TasksAPI.Controllers;

[ApiController]
[Route("api/tasks")]
[Produces("application/json")]
public class TaskController (TaskService taskService) : ControllerBase
{
    private readonly TaskService taskService = taskService;


    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TaskResponse>>> List()
    {
        IEnumerable<TaskResponse> result = await taskService.ListAsync();

        return Ok(result);
    }


    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> GetById(int id)
    {
        TaskResponse? result = await taskService.GetByIdAsync(id);

        if (result is null)
        {
            return NotFound();
        }

        return result;
    }


    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskResponse>> AddAsync([FromBody] TaskRequest request)
    {
        if (!await taskService.CategoryExistsAsync(request.CategoryId))
        {
            return CategoryNotFound(request);
        }

        TaskResponse result = await taskService.AddAsync(request);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }


    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> UpdateAsync(int id, [FromBody] TaskRequest request)
    {
        if (!await taskService.CategoryExistsAsync(request.CategoryId))
        {
            return CategoryNotFound(request);
        }

        TaskResponse? result = await taskService.UpdateAsync(id, request);

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
        bool deleted = await taskService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }


    private ActionResult CategoryNotFound(TaskRequest request)
    {
        ModelState.AddModelError(nameof(request.CategoryId), $"The category {request.CategoryId} doesn't exist.");

        return ValidationProblem(ModelState);
    }
}
