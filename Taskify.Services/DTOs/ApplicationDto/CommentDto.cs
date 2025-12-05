namespace Taskify.Services.DTOs.ApplicationDto
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public UserSummaryDto? User { get; set; }
    }
}