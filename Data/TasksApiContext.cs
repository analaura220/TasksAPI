using Microsoft.EntityFrameworkCore;

namespace TasksAPI.Data
{
    public class TasksApiContext: DbContext
    {
        public TasksApiContext(DbContextOptions<TasksApiContext> opt): base(opt)
        {
            
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Task> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}

