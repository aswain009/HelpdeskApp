using Helpdesk.Api.Data;
using Helpdesk.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Api.Pages.Tickets;

public class DetailsModel(HelpdeskDbContext db) : PageModel
{
    private readonly HelpdeskDbContext _db = db;

    public Ticket Ticket { get; set; } = null!;
    public List<User> Users { get; set; } = new();

    [BindProperty]
    public int? AssignUserId { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var ticket = await _db.Tickets.Include(ticket => ticket.AssignedUser).FirstOrDefaultAsync(ticket => ticket.Id == id);
        if (ticket is null) return NotFound();
        Ticket = ticket;
        Users = await _db.Users.AsNoTracking().OrderBy(user => user.Name).ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAssignAsync(int id)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket is null) return NotFound();
        if (AssignUserId is not null && !await _db.Users.AnyAsync(user => user.Id == AssignUserId))
        {
            ModelState.AddModelError(string.Empty, "Selected user does not exist.");
            return await OnGetAsync(id);
        }
        ticket.AssignedUserId = AssignUserId;
        await _db.SaveChangesAsync();
        return RedirectToPage(new { id });
    }
}
