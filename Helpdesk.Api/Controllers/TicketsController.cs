using Helpdesk.Api.Data;
using Helpdesk.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Helpdesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController(HelpdeskDbContext db) : ControllerBase
{
    public record TicketCreateRequest(
        [param: Required, MaxLength(200)] string Title,
        [param: MaxLength(2000)] string? Description,
        TicketStatus Status,
        int? AssignedUserId
    );

    public record TicketUpdateRequest(
        [param: Required] int Id,
        [param: Required, MaxLength(200)] string Title,
        [param: MaxLength(2000)] string? Description,
        TicketStatus Status,
        int? AssignedUserId
    ); 
    
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Ticket>>> List([FromQuery] TicketStatus? status, [FromQuery] int? assignedUserId)
    {
        var query = db.Tickets
            .Include(ticket => ticket.AssignedUser)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(ticket => ticket.Status == status);
        if (assignedUserId.HasValue)
            query = query.Where(ticket => ticket.AssignedUserId == assignedUserId);

        var items = await query.OrderByDescending(ticket => ticket.UpdatedAt).ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Ticket>> GetById(int id)
    {
        var ticket = await db.Tickets.Include(ticket => ticket.AssignedUser).AsNoTracking().FirstOrDefaultAsync(ticket => ticket.Id == id);
        return ticket is null ? NotFound() : Ok(ticket);
    }

    [HttpPost]
    public async Task<ActionResult<Ticket>> Create(TicketCreateRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        
        if (req.AssignedUserId is not null &&
            !await db.Users.AnyAsync(user => user.Id == req.AssignedUserId))
        {
            return BadRequest("Assigned user does not exist");
        }
        
        var newTicket = new Ticket
        {
            Title = req.Title,
            Description = req.Description,
            Status = req.Status,
            AssignedUserId = req.AssignedUserId
        };

        db.Tickets.Add(newTicket);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = newTicket.Id }, newTicket);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Ticket>> Update(int id, TicketUpdateRequest newTicket)
    {
        if (id != newTicket.Id) return BadRequest("ID mismatch");
        var existing = await db.Tickets.FindAsync(id);
        if (existing is null) return NotFound();
        
        if (newTicket.AssignedUserId is not null &&
            !await db.Users.AnyAsync(user => user.Id == newTicket.AssignedUserId))
        {
            return BadRequest("Assigned user does not exist");
        }
        
        existing.Title = newTicket.Title;
        existing.Description = newTicket.Description;
        existing.Status = newTicket.Status;
        existing.AssignedUserId = newTicket.AssignedUserId;

        await db.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ticket = await db.Tickets.FindAsync(id);
        if (ticket is null) return NotFound();
        db.Tickets.Remove(ticket);
        await db.SaveChangesAsync();
        return NoContent();
    }

    public record AssignRequest(int? AssignedUserId);

    [HttpPost("{id:int}/assign")]
    public async Task<ActionResult<Ticket>> Assign(int id, AssignRequest req)
    {
        var ticket = await db.Tickets.FindAsync(id);
        if (ticket is null) return NotFound();

        if (req.AssignedUserId is not null && !await db.Users.AnyAsync(user => user.Id == req.AssignedUserId))
            return BadRequest("Assigned user does not exist");

        ticket.AssignedUserId = req.AssignedUserId;
        await db.SaveChangesAsync();
        return Ok(ticket);
    }
}
