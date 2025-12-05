namespace Taskify.Domain.Entities
{
    public class Document : BaseEntity
    {
        public string OriginalFileName { get; set;  } = string.Empty;
        public string OriginalFilePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string? PublicId { get; set; }
        public string? Url { get; set; }
        public long Size { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public Guid ProjectId { get; set; }
        public Project? Project { get; set; }
        public string UploadedByUserId { get; set; } = string.Empty;
        public bool IsStarred { get; set; } = false;

    }
}
