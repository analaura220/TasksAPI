using TasksAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace TasksAPI.Repositories
{
    public class TaskRepository(TasksApiContext context)
    {
        private readonly TasksApiContext _context = context;

        public async Task<IEnumerable<Task>> ListAsync()
        {
            IEnumerable<Task> result = await _context.Tasks
                .AsNoTracking()
                .Include(task => task.Category)
                .OrderBy(task => task.Done)
                .ThenBy(task => task.Name)
                .ToListAsync();

            return result;
        }

        public async Task<Task?> GetByIdAsync(int id)
        {
            Task? result = await _context.Tasks
                .AsNoTracking()
                .Include(task => task.Category)
                .FirstOrDefaultAsync(task => task.Id == id);

            return result;
        }

        public async Task<Task> AddAsync(Task task)
        {
            EntityEntry<Task> result = await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();
            await result.Reference(entity => entity.Category).LoadAsync();
            return result.Entity;
        }

        public async Task<Task?> UpdateAsync(int id, Task task)
        {
            Task? result = await _context.Tasks.FindAsync(id);

            if (result is null)
            {
                return null;
            }

            result.Name = task.Name;
            result.Description = task.Description;
            result.Done = task.Done;
            result.CategoryId = task.CategoryId;
            await _context.SaveChangesAsync();
            await _context.Entry(result).Reference(entity => entity.Category).LoadAsync();
            return result;
        }

        public async Task<Task?> DeleteAsync(int id)
        {
            Task? result = await _context.Tasks.FindAsync(id);

            if (result is null)
            {
                return null;
            }

            _context.Tasks.Remove(result);
            await _context.SaveChangesAsync();
            return result;
        }
    }
}
