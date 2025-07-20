using BlogMvc.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlogMvc.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<BlogPost> BlogPosts { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure BlogPost entity
        builder.Entity<BlogPost>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Content)
                .IsRequired()
                .HasMaxLength(5000);

            entity.Property(e => e.Author)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.PublishedDate)
                .IsRequired();

            entity.Property(e => e.UserId)
                .IsRequired();

            // Configure relationship with IdentityUser
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add indexes for performance
            entity.HasIndex(e => e.Title)
                .HasDatabaseName("IX_BlogPosts_Title");

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_BlogPosts_UserId");

            entity.HasIndex(e => e.PublishedDate)
                .HasDatabaseName("IX_BlogPosts_PublishedDate");
        });
    }

}