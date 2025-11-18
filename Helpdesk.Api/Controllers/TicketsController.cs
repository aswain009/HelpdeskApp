using Helpdesk.Api.Data;
using Helpdesk.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController(HelpdeskDbContext db) : ControllerBase
{
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
    public async Task<ActionResult<Ticket>> Create(Ticket ticket)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (ticket.Id != 0) ticket.Id = 0;
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Ticket>> Update(int id, Ticket ticket)
    {
        if (id != ticket.Id) return BadRequest("ID mismatch");
        if (!await db.Tickets.AnyAsync(t => t.Id == id)) return NotFound();
        db.Entry(ticket).State = EntityState.Modified;
        await db.SaveChangesAsync();
        return Ok(ticket);
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
