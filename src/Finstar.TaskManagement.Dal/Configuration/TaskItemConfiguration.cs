using Finstar.TaskManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finstar.TaskManagement.Dal.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(user => user.Id);
        
        builder.Property(user => user.Name)
            .HasColumnType("text")
            .IsRequired();
        
        builder.Property(user => user.SurName)
            .HasColumnType("text")
            .IsRequired();
    }
}


public class TaskItemConfiguration :IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.HasKey(task => task.Id);
        
        builder.Property(task => task.Title).IsRequired();
        
        builder.Property(task => task.Description).IsRequired();
        
        builder.Property(task => task.Status)
            .HasColumnType("text")
            .HasConversion<string>()
            .IsRequired();
        
        builder.Property(task => task.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        
        builder.Property(task => task.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        
        builder.HasOne(task => task.User)
            .WithMany(x => x.Tasks)
            .HasForeignKey(task => task.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}