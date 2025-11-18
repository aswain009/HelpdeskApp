using Helpdesk.Api.Data;
using Helpdesk.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Api.Pages.Tickets;

public class IndexModel(HelpdeskDbContext db) : PageModel
{
    private readonly HelpdeskDbContext _db = db;

    [BindProperty(SupportsGet = true)]
    public TicketStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? AssignedUserId { get; set; }

    public List<User> Users { get; set; } = new();
    public List<Ticket> Tickets { get; set; } = new();

    public async Task OnGetAsync()
    {
        Users = await _db.Users.AsNoTracking().OrderBy(user => user.Name).ToListAsync();

        var queryableEntity = _db.Tickets.Include(ticket => ticket.AssignedUser).AsNoTracking().AsQueryable();
        if (Status is not null) queryableEntity = queryableEntity.Where(ticket => ticket.Status == Status);
        if (AssignedUserId is not null) queryableEntity = queryableEntity.Where(ticket => ticket.AssignedUserId == AssignedUserId);
        Tickets = await queryableEntity.OrderByDescending(ticket => ticket.UpdatedAt).ToListAsync();
    }
}
