using FluentAssertions;
using Helpdesk.Api.Data;
using Helpdesk.Api.Models;
using Helpdesk.Api.Pages.Tickets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Xunit;

namespace Helpdesk.Tests;

public class RazorPagesTests
{
    private static HelpdeskDbContext CreateInMemory()
    {
        var opts = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new HelpdeskDbContext(opts);
    }

    [Fact]
    public async Task EditModel_OnGet_Loads_Ticket_And_Users()
    {
        await using var db = CreateInMemory();
        var mockUser = new User { Name = "User A", Email = "a@example.com" };
        db.Users.Add(mockUser);
        var mockTicket = new Ticket { Title = "T1", AssignedUserId = null };
        db.Tickets.Add(mockTicket);
        await db.SaveChangesAsync();

        var page = new EditModel(db);
        var result = await page.OnGetAsync(mockTicket.Id);

        result.Should().BeOfType<PageResult>();
        page.Users.Should().HaveCount(1);
        page.Ticket.Id.Should().Be(mockTicket.Id);
        page.Ticket.Title.Should().Be("T1");
    }

    [Fact]
    public async Task EditModel_OnPost_Updates_Ticket_Fields()
    {
        await using var db = CreateInMemory();
        db.Users.Add(new User { Name = "U1", Email = "u1@example.com" });
        var mockTicket = new Ticket { Title = "Orig", Description = "D", Status = TicketStatus.Open };
        db.Tickets.Add(mockTicket);
        await db.SaveChangesAsync();

        var page = new EditModel(db)
        {
            Ticket = new EditModel.InputModel
            {
                Id = mockTicket.Id,
                Title = "Changed",
                Description = "New D",
                Status = TicketStatus.InProgress,
                AssignedUserId = null
            }
        };

        var result = await page.OnPostAsync(mockTicket.Id);
        result.Should().BeOfType<RedirectToPageResult>();

        var reloaded = await db.Tickets.FindAsync(mockTicket.Id);
        reloaded!.Title.Should().Be("Changed");
        reloaded.Description.Should().Be("New D");
        reloaded.Status.Should().Be(TicketStatus.InProgress);
    }

    [Fact]
    public async Task DetailsModel_OnPostAssign_Assigns_And_Unassigns()
    {
        await using var db = CreateInMemory();
        var mockUser = new User { Name = "UU", Email = "uu@example.com" };
        db.Users.Add(mockUser);
        var mockTicket = new Ticket { Title = "T" };
        db.Tickets.Add(mockTicket);
        await db.SaveChangesAsync();

        var page = new DetailsModel(db)
        {
            AssignUserId = mockUser.Id
        };
        var assignResult = await page.OnPostAssignAsync(mockTicket.Id);
        assignResult.Should().BeOfType<RedirectToPageResult>();
        (await db.Tickets.FindAsync(mockTicket.Id))!.AssignedUserId.Should().Be(mockUser.Id);

        page.AssignUserId = null;
        var unassignResult = await page.OnPostAssignAsync(mockTicket.Id);
        unassignResult.Should().BeOfType<RedirectToPageResult>();
        (await db.Tickets.FindAsync(mockTicket.Id))!.AssignedUserId.Should().BeNull();
    }
}
