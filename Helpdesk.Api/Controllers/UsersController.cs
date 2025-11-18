using Helpdesk.Api.Data;
using Helpdesk.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(HelpdeskDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> Get() =>
        await db.Users.AsNoTracking().OrderBy(user => user.Name).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<User>> GetById(int id)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == id);
        return user is null ? NotFound() : Ok(user);
    }
}
