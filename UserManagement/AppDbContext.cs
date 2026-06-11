using Microsoft.EntityFrameworkCore;
using UserManagement.Models;

namespace UserManagement;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users");
            
            e.HasKey(x => x.Id);

            e.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(100);
            
            e.HasIndex(x => x.Email).IsUnique();

            e.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            e.Property(x => x.Password)
                .IsRequired()
                .HasMaxLength(256);
            
            e.Property(x => x.UserStatus)
                .IsRequired();
            
            e.Property(x => x.CreatedAt)
                .IsRequired()
                .HasColumnType("timestamp without time zone");
            
            e.Property(x => x.LastLogin)
                .HasColumnType("timestamp without time zone");
        });
    }
}