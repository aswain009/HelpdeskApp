using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Helpdesk.Api.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        return RedirectToPage("/Tickets/Index");
    }
}
