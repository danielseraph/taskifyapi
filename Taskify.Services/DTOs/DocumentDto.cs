namespace Taskify.Services.DTOs
{
    public class DocumentDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public Guid ProjectId { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool IsStarred { get; set; }
        public DateTime CreateAt { get; set; }
        public string UploadedBy { get; set; } = string.Empty;
    }
}
