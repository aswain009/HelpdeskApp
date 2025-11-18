using Helpdesk.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Api.Data;

public class HelpdeskDbContext(DbContextOptions<HelpdeskDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Ticket> Tickets => Set<Ticket>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<Ticket>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
