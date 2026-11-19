using Microsoft.EntityFrameworkCore;

namespace Finstar.TaskManagement.ChangeTracker.Dal;

public interface ITaskChangesStorage
{
    DbSet<TaskChange> TaskChanges { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class TaskChangesContext(DbContextOptions<TaskChangesContext> options)
    : DbContext(options), ITaskChangesStorage
{
    public DbSet<TaskChange> TaskChanges { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskChangesContext).Assembly);
    }
}