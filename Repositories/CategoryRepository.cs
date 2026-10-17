using TasksAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace TasksAPI.Repositories
{
    public class CategoryRepository(TasksApiContext context)
    {
        private readonly TasksApiContext _context = context;

        public async Task<IEnumerable<Category>> ListAsync()
        {
            IEnumerable<Category> result = await _context.Categories
                .AsNoTracking()
                .OrderBy(category => category.Name)
                .ToListAsync();

            return result;
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            Category? result = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(category => category.Id == id);

            return result;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            bool result = await _context.Categories.AnyAsync(category => category.Id == id);

            return result;
        }

        public async Task<Category> AddAsync(Category category)
        {
            EntityEntry<Category> result = await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<Category?> UpdateAsync(int id, Category category)
        {
            Category? result = await _context.Categories.FindAsync(id);

            if (result is null)
            {
                return null;
            }

            result.Name = category.Name;
            await _context.SaveChangesAsync();
            return result;
        }

        public async Task<Category?> DeleteAsync(int id)
        {
            Category? result = await _context.Categories.FindAsync(id);

            if (result is null)
            {
                return null;
            }

            _context.Categories.Remove(result);
            await _context.SaveChangesAsync();
            return result;
        }
    }
}
