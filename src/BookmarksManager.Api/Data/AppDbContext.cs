using BookmarksManager.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookmarksManager.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<BookmarkTag> BookmarkTags => Set<BookmarkTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Folder>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Name).IsRequired();
            entity.HasOne(f => f.Parent)
                .WithMany(f => f.Children)
                .HasForeignKey(f => f.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Bookmark>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Title).IsRequired();
            entity.Property(b => b.Url).IsRequired();
            entity.Property(b => b.Description).HasDefaultValue(string.Empty);
            entity.Property(b => b.Thumbnail).HasDefaultValue(string.Empty);
            entity.Property(b => b.Favorite).HasDefaultValue(false);
            entity.HasOne(b => b.Folder)
                .WithMany(f => f.Bookmarks)
                .HasForeignKey(b => b.FolderId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired();
            entity.HasIndex(t => t.Name).IsUnique();
        });

        modelBuilder.Entity<BookmarkTag>(entity =>
        {
            entity.HasKey(bt => new { bt.BookmarkId, bt.TagId });
            entity.HasOne(bt => bt.Bookmark)
                .WithMany(b => b.BookmarkTags)
                .HasForeignKey(bt => bt.BookmarkId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(bt => bt.Tag)
                .WithMany(t => t.BookmarkTags)
                .HasForeignKey(bt => bt.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
