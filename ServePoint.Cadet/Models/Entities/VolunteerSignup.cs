using System.ComponentModel.DataAnnotations;

namespace ServePoint.Cadet.Models.Entities;

public class VolunteerSignup
{
    public int Id { get; set; }

    [Required]
    public int VolunteerOpportunityId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public DateTime SignedUpAt { get; set; } = DateTime.UtcNow;

    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public VolunteerOpportunity VolunteerOpportunity { get; set; } = null!;
}