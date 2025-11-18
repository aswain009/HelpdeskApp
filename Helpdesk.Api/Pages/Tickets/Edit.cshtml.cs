using Helpdesk.Api.Data;
using Helpdesk.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Helpdesk.Api.Pages.Tickets;

public class EditModel(HelpdeskDbContext db) : PageModel
{
    private readonly HelpdeskDbContext _db = db;

    [BindProperty]
    public InputModel Ticket { get; set; } = new();

    public List<User> Users { get; set; } = new();

    public class InputModel
    {
        public int Id { get; set; }
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        [MaxLength(2000)]
        public string? Description { get; set; }
        public TicketStatus Status { get; set; } = TicketStatus.Open;
        public int? AssignedUserId { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Users = await _db.Users.AsNoTracking().OrderBy(user => user.Name).ToListAsync();
        var entity = await _db.Tickets.AsNoTracking().FirstOrDefaultAsync(ticket => ticket.Id == id);
        if (entity is null) return NotFound();
        Ticket = new InputModel
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            Status = entity.Status,
            AssignedUserId = entity.AssignedUserId
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        Users = await _db.Users.AsNoTracking().OrderBy(user => user.Name).ToListAsync();
        if (!ModelState.IsValid) return Page();
        if (id != Ticket.Id) return BadRequest();

        var entity = await _db.Tickets.FirstOrDefaultAsync(ticket => ticket.Id == id);
        if (entity is null) return NotFound();

        entity.Title = Ticket.Title;
        entity.Description = Ticket.Description;
        entity.Status = Ticket.Status;
        entity.AssignedUserId = Ticket.AssignedUserId;

        await _db.SaveChangesAsync();
        return RedirectToPage("Details", new { id });
    }
}
