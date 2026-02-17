using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServePoint.Cadet.Models.Entities;

public class VolunteerSignup
{
    public int Id { get; set; }

    [Required]
    public int VolunteerOpportunityId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public DateTime SignedUpAt { get; set; } = DateTime.UtcNow;
    
    public bool AttendanceSubmitted { get; set; }
    public DateTime? AttendanceSubmittedAt { get; set; }

    public bool AttendanceApproved { get; set; }
    public DateTime? AttendanceApprovedAt { get; set; }
    public string? ApprovedByUserId { get; set; }

    [NotMapped]
    public string? DisplayName { get; set; }

    // Navigation
    public VolunteerOpportunity VolunteerOpportunity { get; set; } = null!;
}