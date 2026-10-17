namespace TasksAPI.Dtos;

public record TaskResponse(int Id, string Name, string Description, bool Done, int CategoryId, string? CategoryName)
{
    public static TaskResponse FromEntity(Task task)
    {
        return new TaskResponse(task.Id, task.Name, task.Description, task.Done, task.CategoryId, task.Category?.Name);
    }
}
