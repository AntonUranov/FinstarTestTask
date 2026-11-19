using Finstar.TaskManagement.Dal;
using Finstar.TaskManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Finstar.TaskManagement.Api;

public interface IUserSeedService
{
    Task SeedAsync();
}

public class UserSeedService : IUserSeedService
{
    private readonly ITaskManagementStorage _storage;
    private readonly ILogger<UserSeedService> _logger;

    public UserSeedService(ITaskManagementStorage storage, ILogger<UserSeedService> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.Log(LogLevel.Information,  "Users seeding is started");
            
            await AddUserIfNotExists(Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), "David ", "Blaine");

            await _storage.SaveChangesAsync();
            
            _logger.Log(LogLevel.Information,  "Users seeding is finished");
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e, "Users seeding is failed");
            throw;
        }
    }
    
    private async Task AddUserIfNotExists(Guid id, string name, string surname)
    {
        if (!await _storage.Users.AnyAsync(x => x.Id == id))
        {
            _storage.Users.Add(new User {Id = id, Name = name, SurName =  surname});
        }
    }
}