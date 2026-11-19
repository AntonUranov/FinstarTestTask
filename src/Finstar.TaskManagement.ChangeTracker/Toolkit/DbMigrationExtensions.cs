using Finstar.TaskManagement.ChangeTracker.Dal;
using Microsoft.EntityFrameworkCore;

namespace Finstar.TaskManagement.Api.Toolkit;

public static class DbMigrationExtensions
{
    public static async Task MigrateDatabase(this IServiceProvider serviceProvider)
    {
        using var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        await using var dictionaryContext = serviceScope.ServiceProvider.GetRequiredService<TaskChangesContext>();
        await dictionaryContext.Database.MigrateAsync();
    }
}