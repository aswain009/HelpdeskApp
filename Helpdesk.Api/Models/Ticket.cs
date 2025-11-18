using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Helpdesk.Api.Models;

public class Ticket
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public TicketStatus Status { get; set; } = TicketStatus.Open;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public int? AssignedUserId { get; set; }

    [ForeignKey(nameof(AssignedUserId))]
    public User? AssignedUser { get; set; }
}
