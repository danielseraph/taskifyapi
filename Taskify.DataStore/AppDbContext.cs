using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Taskify.Domain.Entities;

namespace Taskify.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Project> Projects { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<UserProject> UserProjects { get; set; }
        public DbSet<UserTask> UserTasks { get; set; }
        public DbSet<Comments> Comments { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Document> Documents { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserProject>()
                   .HasKey(up => new { up.UserId, up.ProjectId });

            builder.Entity<UserTask>()
           .HasKey(ut => new { ut.UserId, ut.TaskItemId });



            base.OnModelCreating(builder);

            builder.Entity<UserTask>()
                .HasKey(ut => new { ut.UserId, ut.TaskItemId });

            builder.Entity<UserTask>()
                .HasOne(ut => ut.User)
                .WithMany(u => u.UserTasks)
                .HasForeignKey(ut => ut.UserId);

            builder.Entity<UserTask>()
                .HasOne(ut => ut.TaskItem)
                .WithMany(t => t.UserTasks)
                .HasForeignKey(ut => ut.TaskItemId);

            builder.Entity<RefreshToken>()
                .HasOne(rt => rt.AppUser)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}

