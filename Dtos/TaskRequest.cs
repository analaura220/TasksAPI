using System.ComponentModel.DataAnnotations;

namespace TasksAPI.Dtos;

public record TaskRequest(
    [Required, StringLength(100)] string Name,
    [StringLength(500)] string? Description,
    bool Done,
    [Range(1, int.MaxValue)] int CategoryId
)
{
    public Task ToEntity()
    {
        return new Task
        {
            Name = Name,
            Description = Description ?? string.Empty,
            Done = Done,
            CategoryId = CategoryId
        };
    }
}
