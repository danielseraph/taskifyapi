using System.ComponentModel.DataAnnotations.Schema;

namespace Taskify.Domain.Entities
{
    public class Photo
    {
        public Guid Id { get; set; }
        public string? Url { get; set; }
        public string? PublicId { get; set; }
        public Guid AppUserId { get; set; }

        [ForeignKey("AppUserId")]
        public AppUser? AppUser { get; set; }

    }
}
