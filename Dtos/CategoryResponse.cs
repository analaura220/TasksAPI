namespace TasksAPI.Dtos;

public record CategoryResponse(int Id, string Name)
{
    public static CategoryResponse FromEntity(Category category)
    {
        return new CategoryResponse(category.Id, category.Name);
    }
}
