using Microsoft.EntityFrameworkCore;
using web.Models.Entities;

namespace web.Services.Db;

public class UserSessionStoreDbContext : DbContext
{
    public DbSet<UserSession>? UserSessions { get; set; }
    public DbSet<PersonEventSubscription>? PeopleEventSubscriptions { get; set; }

    public UserSessionStoreDbContext(DbContextOptions<UserSessionStoreDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserSession>()
            .HasKey(x => x.ConnectionId);

        modelBuilder.Entity<UserSession>()
            .HasMany(x => x.PersonEventSubscriptions)
            .WithOne(x => x.Session)
            .HasForeignKey(x => x.ConnectionId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}