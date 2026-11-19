using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finstar.TaskManagement.ChangeTracker.Dal;

public class TaskChangeConfiguration : IEntityTypeConfiguration<TaskChange>
{
    public void Configure(EntityTypeBuilder<TaskChange> builder)
    {
        builder.HasKey(taskChange => taskChange.TaskId);
        
        builder.Property(taskChange => taskChange.Title)
            .HasColumnType("text")
            .IsRequired();
        
        builder.Property(taskChange => taskChange.Description)
            .HasColumnType("text")
            .IsRequired();
        
        builder.Property(taskChange => taskChange.ChangeType)
            .HasColumnType("text")
            .IsRequired();
        
        builder.Property(task => task.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        
        builder.Property(task => task.EventId)
            .HasColumnType("uuid")
            .IsRequired();
    }
}
