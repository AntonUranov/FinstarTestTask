using Finstar.TaskManagement.Models.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Finstar.TaskManagement.Dal;

public class TaskManagementContext(DbContextOptions<TaskManagementContext> options)
    : DbContext(options), ITaskManagementStorage
{
    public DbSet<TaskItem> TaskItems { get; set; }
    
    public DbSet<User> Users { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskManagementContext).Assembly);
        
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}