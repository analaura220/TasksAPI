using TasksAPI.Dtos;
using TasksAPI.Repositories;

namespace TasksAPI.Services;

public class CategoryService(CategoryRepository categoryRepository)
{
    private readonly CategoryRepository _categoryRepository = categoryRepository;

    public async Task<IEnumerable<CategoryResponse>> ListAsync()
    {
        IEnumerable<Category> result = await _categoryRepository.ListAsync();

        return result.Select(CategoryResponse.FromEntity);
    }

    public async Task<CategoryResponse?> GetByIdAsync(int id)
    {
        Category? result = await _categoryRepository.GetByIdAsync(id);

        return result is null ? null : CategoryResponse.FromEntity(result);
    }

    public async Task<CategoryResponse> AddAsync(CategoryRequest request)
    {
        Category result = await _categoryRepository.AddAsync(new Category { Name = request.Name });

        return CategoryResponse.FromEntity(result);
    }

    public async Task<CategoryResponse?> UpdateAsync(int id, CategoryRequest request)
    {
        Category? result = await _categoryRepository.UpdateAsync(id, new Category { Name = request.Name });

        return result is null ? null : CategoryResponse.FromEntity(result);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Category? result = await _categoryRepository.DeleteAsync(id);

        return result is not null;
    }

}
