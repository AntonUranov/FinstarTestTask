using Finstar.TaskManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Finstar.TaskManagement.Dal;

public interface ITaskManagementStorage
{
    DbSet<TaskItem> TaskItems { get; }
    
    DbSet<User> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}