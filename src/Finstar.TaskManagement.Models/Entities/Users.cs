namespace Finstar.TaskManagement.Models.Entities;

public class User
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string SurName { get; set; } = string.Empty;

    public List<TaskItem>? Tasks { get; set; }
}