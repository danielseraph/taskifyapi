namespace Taskify.Domain.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreateAT { get; set; }= DateTime.UtcNow;
        public DateTime UpdateAT { get; set; } = DateTime.UtcNow;
    }
}