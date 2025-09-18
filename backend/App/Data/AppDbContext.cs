using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using App.Domain;
using NetTopologySuite.Geometries;

namespace App.Data;

public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Profile> Profiles { get; set; } = null!;
    public DbSet<Photo> Photos { get; set; } = null!;
    public DbSet<Swipe> Swipes { get; set; } = null!;
    public DbSet<Match> Matches { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<Block> Blocks { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Profile configuration
        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Bio).HasMaxLength(500);
            entity.Property(e => e.Location).HasColumnType("geography(Point,4326)");
            entity.HasIndex(e => e.Location).HasMethod("GIST");

            entity.HasOne(e => e.User)
                  .WithOne(u => u.Profile)
                  .HasForeignKey<Profile>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Photo configuration
        modelBuilder.Entity<Photo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Data).IsRequired();
            entity.ToTable(t => t.HasCheckConstraint("CK_Photo_SizeBytes", "\"SizeBytes\" <= 5242880"));

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Photos)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Swipe configuration
        modelBuilder.Entity<Swipe>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SwiperId, e.TargetId }).IsUnique();
            entity.HasIndex(e => e.SwiperId);
            entity.HasIndex(e => e.TargetId);

            entity.HasOne(e => e.Swiper)
                  .WithMany(u => u.SwipesMade)
                  .HasForeignKey(e => e.SwiperId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Target)
                  .WithMany(u => u.SwipesReceived)
                  .HasForeignKey(e => e.TargetId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Match configuration
        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Simple unique index for user pairs (we'll handle uniqueness in application logic)
            entity.HasIndex(e => new { e.UserAId, e.UserBId }).IsUnique();

            entity.HasOne(e => e.UserA)
                  .WithMany()
                  .HasForeignKey(e => e.UserAId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UserB)
                  .WithMany()
                  .HasForeignKey(e => e.UserBId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Message configuration
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.MatchId, e.SentAt });

            entity.HasOne(e => e.Match)
                  .WithMany(m => m.Messages)
                  .HasForeignKey(e => e.MatchId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Sender)
                  .WithMany()
                  .HasForeignKey(e => e.SenderId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ImagePhoto)
                  .WithMany()
                  .HasForeignKey(e => e.ImagePhotoId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Block configuration
        modelBuilder.Entity<Block>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.BlockerId, e.TargetId }).IsUnique();

            entity.HasOne(e => e.Blocker)
                  .WithMany(u => u.BlocksMade)
                  .HasForeignKey(e => e.BlockerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Target)
                  .WithMany(u => u.BlocksReceived)
                  .HasForeignKey(e => e.TargetId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();

            entity.HasOne(e => e.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Customize Identity table names to snake_case
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<IdentityRole<Guid>>().ToTable("roles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims");

        modelBuilder.Entity<Profile>().ToTable("profiles");
        modelBuilder.Entity<Photo>().ToTable("photos");
        modelBuilder.Entity<Swipe>().ToTable("swipes");
        modelBuilder.Entity<Match>().ToTable("matches");
        modelBuilder.Entity<Message>().ToTable("messages");
        modelBuilder.Entity<Block>().ToTable("blocks");
        modelBuilder.Entity<RefreshToken>().ToTable("refresh_tokens");
    }
}