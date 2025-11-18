using Helpdesk.Api.Data;
using Helpdesk.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Helpdesk.Api.Pages.Tickets;

public class CreateModel(HelpdeskDbContext db) : PageModel
{
    private readonly HelpdeskDbContext _db = db;

    [BindProperty]
    public InputModel Ticket { get; set; } = new();

    public List<User> Users { get; set; } = new();

    public class InputModel
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [MaxLength(2000)]
        public string? Description { get; set; }
        
        public TicketStatus Status { get; set; } = TicketStatus.Open;
        
        public int? AssignedUserId { get; set; }
    }

    public async Task OnGetAsync()
    {
        Users = await _db.Users.AsNoTracking().OrderBy(user => user.Name).ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Users = await _db.Users.AsNoTracking().OrderBy(user => user.Name).ToListAsync();
        if (!ModelState.IsValid) return Page();

        var entity = new Ticket
        {
            Title = Ticket.Title,
            Description = Ticket.Description,
            Status = Ticket.Status,
            AssignedUserId = Ticket.AssignedUserId
        };

        _db.Tickets.Add(entity);
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
