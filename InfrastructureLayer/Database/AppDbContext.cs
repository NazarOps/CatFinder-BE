using DomainLayer.Models;
using DomainLayer.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Database
{
    /// <summary>
    /// The main EF Core database context for CatFinder.
    /// Declares every entity as a table and configures relationships
    /// that EF Core cannot infer automatically from conventions.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Cat> Cats => Set<Cat>();
        public DbSet<Advertisement> Advertisements => Set<Advertisement>();
        public DbSet<AdvertisementImage> AdvertisementImages => Set<AdvertisementImage>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<SavedAdvertisement> SavedAdvertisements => Set<SavedAdvertisement>();
        public DbSet<Report> Reports => Set<Report>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Account ────────────────────────────────────────────────────────────
            // Email and username must be unique across all accounts.
            modelBuilder.Entity<Account>()
                .HasIndex(a => a.Email).IsUnique();
            modelBuilder.Entity<Account>()
                .HasIndex(a => a.Username).IsUnique();

            // ── Cat → Account ──────────────────────────────────────────────────────
            // Deleting an account removes all their registered cats.
            modelBuilder.Entity<Cat>()
                .HasOne(c => c.Account)
                .WithMany(a => a.Cats)
                .HasForeignKey(c => c.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Advertisement → Account ────────────────────────────────────────────
            // Deleting an account removes all their advertisements.
            modelBuilder.Entity<Advertisement>()
                .HasOne(a => a.Account)
                .WithMany(a => a.Advertisements)
                .HasForeignKey(a => a.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Advertisement>()
                .Property(a => a.ModerationStatus)
                .HasDefaultValue(ModerationStatus.Pending);

            modelBuilder.Entity<Advertisement>()
                .Property(a => a.IsVisible)
                .HasDefaultValue(false);

            // ── Advertisement → Cat ────────────────────────────────────────────────
            // Restrict prevents SQL Server from raising a multiple-cascade-paths error.
            // (Account already cascades to both Cat and Advertisement.)
            modelBuilder.Entity<Advertisement>()
                .HasOne(a => a.Cat)
                .WithMany(c => c.Advertisements)
                .HasForeignKey(a => a.CatId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Advertisement → Location ───────────────────────────────────────────
            // Locations are shared, so deleting a location is blocked if ads reference it.
            modelBuilder.Entity<Advertisement>()
                .HasOne(a => a.Location)
                .WithMany(l => l.Advertisements)
                .HasForeignKey(a => a.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Comment → Advertisement ────────────────────────────────────────────
            // Deleting an advertisement removes all its comments.
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Advertisement)
                .WithMany(a => a.Comments)
                .HasForeignKey(c => c.AdvertisementId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Comment → Account ──────────────────────────────────────────────────
            // Restrict avoids a second cascade path: Account → Advertisement → Comment
            // already exists, so Account → Comment must not also cascade.
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Account)
                .WithMany(a => a.Comments)
                .HasForeignKey(c => c.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Comment self-reference (replies) ───────────────────────────────────
            // A comment can optionally be a reply to another comment.
            // Restrict so deleting a parent comment doesn't silently wipe all replies.
            modelBuilder.Entity<Comment>()
                .HasOne<Comment>()
                .WithMany()
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── AdvertisementImage → Advertisement ─────────────────────────────────
            // Deleting an advertisement removes all its images.
            modelBuilder.Entity<AdvertisementImage>()
                .HasOne(i => i.Advertisement)
                .WithMany(a => a.AdvertisementImages)
                .HasForeignKey(i => i.AdvertisementId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── SavedAdvertisement → Account ───────────────────────────────────────
            // Deleting an account removes their saved/bookmarked advertisements.
            modelBuilder.Entity<SavedAdvertisement>()
                .HasOne(s => s.Account)
                .WithMany(a => a.SavedAdvertisements)
                .HasForeignKey(s => s.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── SavedAdvertisement → Advertisement ─────────────────────────────────
            // Restrict avoids a second cascade path to SavedAdvertisement.
            // (Account already cascades: Account → SavedAdvertisement.)
            modelBuilder.Entity<SavedAdvertisement>()
                .HasOne(s => s.Advertisement)
                .WithMany(a => a.SavedAdvertisements)
                .HasForeignKey(s => s.AdvertisementId)
                .OnDelete(DeleteBehavior.Restrict);

            // An account can only bookmark the same advertisement once.
            modelBuilder.Entity<SavedAdvertisement>()
                .HasIndex(s => new { s.AccountId, s.AdvertisementId })
                .IsUnique();

            // ── Report → Advertisement ─────────────────────────────────────────────
            // Deleting an advertisement removes its reports.
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Advertisement)
                .WithMany(a => a.Reports)
                .HasForeignKey(r => r.AdvertisementId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Report → Account ───────────────────────────────────────────────────
            // Restrict to avoid second cascade path via Account → Advertisement → Report.
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Account)
                .WithMany()
                .HasForeignKey(r => r.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // CommentId is a plain nullable int — no FK constraint is configured because
            // both Advertisement → Report (CASCADE) and Advertisement → Comment → Report
            // would create multiple cascade paths, which SQL Server rejects.
            // Reports are audit records; a stale CommentId after comment deletion is fine.
            modelBuilder.Entity<Report>()
                .Property(r => r.CommentId)
                .IsRequired(false);

            // One report per user per advertisement-or-comment combination.
            modelBuilder.Entity<Report>()
                .HasIndex(r => new { r.AccountId, r.AdvertisementId, r.CommentId })
                .IsUnique();

            // ── Location coordinate precision ──────────────────────────────────────
            // 9 digits total, 6 decimal places — accurate to ~0.1 metre.
            modelBuilder.Entity<Location>()
                .Property(l => l.Latitude)
                .HasPrecision(9, 6);
            modelBuilder.Entity<Location>()
                .Property(l => l.Longitude)
                .HasPrecision(9, 6);
        }
    }
}
