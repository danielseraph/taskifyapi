namespace Taskify.Domain.Entities
{
    public class UserTask
    {
        public string? UserId { get; set; }
        public AppUser User { get; set; } = default!;

        public Guid TaskItemId { get; set; }
        public TaskItem TaskItem { get; set; } = default!;

    }
}
