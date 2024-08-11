using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        // Configure the relationship between TaskItem and TaskList
        builder.HasOne(taskItem => taskItem.TaskList) // TaskItem has one TaskList
               .WithMany(taskList => taskList.TaskItems) // TaskList has many TaskItems
               .HasForeignKey(taskItem => taskItem.TaskListId) // Foreign key in TaskItem
               .OnDelete(DeleteBehavior.Cascade); // Define the delete behavior if necessary

        // Configure the unique constraint on Title within the context of TaskList
        builder.HasIndex(taskItem => new { taskItem.TaskListId, taskItem.Title })
               .IsUnique(); // Ensure that Title is unique within each TaskList
    }
}
