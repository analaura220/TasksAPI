using TasksAPI.Dtos;
using TasksAPI.Repositories;

namespace TasksAPI.Services;

public class TaskService(TaskRepository taskRepository, CategoryRepository categoryRepository)
{
    private readonly TaskRepository _taskRepository = taskRepository;
    private readonly CategoryRepository _categoryRepository = categoryRepository;

    public async Task<IEnumerable<TaskResponse>> ListAsync()
    {
        IEnumerable<Task> result = await _taskRepository.ListAsync();

        return result.Select(TaskResponse.FromEntity);
    }

    public async Task<TaskResponse?> GetByIdAsync(int id)
    {
        Task? result = await _taskRepository.GetByIdAsync(id);

        return result is null ? null : TaskResponse.FromEntity(result);
    }

    public async Task<bool> CategoryExistsAsync(int categoryId)
    {
        bool result = await _categoryRepository.ExistsAsync(categoryId);

        return result;
    }

    public async Task<TaskResponse> AddAsync(TaskRequest request)
    {
        Task result = await _taskRepository.AddAsync(request.ToEntity());

        return TaskResponse.FromEntity(result);
    }

    public async Task<TaskResponse?> UpdateAsync(int id, TaskRequest request)
    {
        Task? result = await _taskRepository.UpdateAsync(id, request.ToEntity());

        return result is null ? null : TaskResponse.FromEntity(result);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Task? result = await _taskRepository.DeleteAsync(id);

        return result is not null;
    }
}
