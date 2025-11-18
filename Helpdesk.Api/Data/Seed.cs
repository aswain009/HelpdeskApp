using Helpdesk.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Api.Data;

public static class Seed
{
    public static async Task EnsureSeedDataAsync(HelpdeskDbContext db)
    {
        if (!await db.Users.AnyAsync())
        {
            db.Users.AddRange(
                new User { Name = "Alice Admin", Email = "alice@example.com" },
                new User { Name = "Bob Builder", Email = "bob@example.com" },
                new User { Name = "Carol Support", Email = "carol@example.com" }
            );
            await db.SaveChangesAsync();
        }

        if (!await db.Tickets.AnyAsync())
        {
            var users = await db.Users.ToListAsync();
            db.Tickets.AddRange(
                new Ticket
                {
                    Title = "Cannot connect to VPN",
                    Description = "User reports VPN client failing to connect.",
                    Status = TicketStatus.Open,
                    AssignedUserId = users.FirstOrDefault()?.Id
                },
                new Ticket
                {
                    Title = "Email not syncing",
                    Description = "Mobile device not syncing emails.",
                    Status = TicketStatus.InProgress,
                    AssignedUserId = users.Skip(1).FirstOrDefault()?.Id
                }
            );
            await db.SaveChangesAsync();
        }
    }
}
