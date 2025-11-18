using FluentAssertions;
using Helpdesk.Api.Data;
using Helpdesk.Api.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Helpdesk.Tests;

public class TimestampsTests
{
    private static HelpdeskDbContext CreateInMemory()
    {
        var opts = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new HelpdeskDbContext(opts);
    }

    [Fact]
    public async Task CreatedAt_And_UpdatedAt_Set_On_Create()
    {
        await using var db = CreateInMemory();
        db.Tickets.Add(new Ticket { Title = "first" });
        await db.SaveChangesAsync();

        var mockTicket = await db.Tickets.FirstAsync();
        mockTicket.CreatedAt.Should().NotBe(default);
        mockTicket.UpdatedAt.Should().NotBe(default);
        mockTicket.UpdatedAt.Should().Be(mockTicket.CreatedAt);
    }

    [Fact]
    public async Task UpdatedAt_Changes_On_Update()
    {
        await using var db = CreateInMemory();
        var mockTicket = new Ticket { Title = "first" };
        db.Tickets.Add(mockTicket);
        await db.SaveChangesAsync();
        var originalUpdated = mockTicket.UpdatedAt;

        mockTicket.Title = "changed";
        await Task.Delay(5); // ensure clock moves
        await db.SaveChangesAsync();

        mockTicket.UpdatedAt.Should().BeAfter(originalUpdated);
    }
}
