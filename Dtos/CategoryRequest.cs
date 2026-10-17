using System.ComponentModel.DataAnnotations;

namespace TasksAPI.Dtos;

public record CategoryRequest(
    [Required, StringLength(100)] string Name
);
