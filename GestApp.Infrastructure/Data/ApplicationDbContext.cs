using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GestApp.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //modelBuilder.Entity<ApplicationUser>()
        //    .HasIndex(u => u.Email)
        //    .IsUnique();

        //modelBuilder.Entity<ApplicationUser>()
        //    .HasIndex(u => u.FiscalCode)
        //    .IsUnique();
    }

    //public DbSet<AuditDocument> AuditDocuments { get; set; } = null!;
    //public DbSet<AuditDocumentChapter> AuditDocumentChapters { get; set; } = null!;
    //public DbSet<AuditDocumentSubChapter> AuditDocumentSubChapters { get; set; } = null!;
    //public DbSet<AuditDocumentFileUpload> AuditDocumentFileUploads { get; set; } = null!;
    //public DbSet<Notification> Notifications { get; set; } = null!;
    //public DbSet<UserNotification> UserNotifications { get; set; } = null!;
    //public DbSet<AuditLog> AuditLog { get; set; } = null!;
}
