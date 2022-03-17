using Microsoft.EntityFrameworkCore;
using web.Models;

namespace web.Services.Db;

public class UserSessionStoreDbContext : DbContext
{
    public DbSet<UserSessionRecord>? UserSessions { get; set; }
    public DbSet<PeopleEventSubscription>? PeopleEvenSubscriptions { get; set; }

    public UserSessionStoreDbContext(DbContextOptions<UserSessionStoreDbContext> options) : base(options)
    {

    }
}