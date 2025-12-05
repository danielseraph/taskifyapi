namespace Taskify.Domain.Entities
{
    public class Comments : BaseEntity
    {
        public string Content { get; set; } = default!;
        public Guid TaskItemId { get; set; }
        public TaskItem TaskItem { get; set; } = default!;

        public Guid AuthorId { get; set; }
        public AppUser Author { get; set; } = default!;
    }
}
