using Microsoft.EntityFrameworkCore;
using SimpleWallet.Models;

namespace SimpleWallet.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasIndex(w => w.UserId).IsUnique();
            entity.HasIndex(w => w.Address).IsUnique();

            entity.HasOne(w => w.User)
                  .WithOne(u => u.Wallet)
                  .HasForeignKey<Wallet>(w => w.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(w => w.Balance).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.Property(t => t.Amount).HasPrecision(18, 2);

            entity.HasOne(t => t.SenderUser)
                  .WithMany()
                  .HasForeignKey(t => t.SenderUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.RecipientUser)
                  .WithMany()
                  .HasForeignKey(t => t.RecipientUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
