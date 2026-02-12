using System.ComponentModel.DataAnnotations;

namespace ServePoint.Cadet.Models.Entities;

public class VolunteerOpportunity
{
    public int Id { get; set; }

    public int ContactId { get; set; }

    public Contact Contact { get; set; } = null!;


    [Required, MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime Date { get; set; }
    
    public TimeOnly StartTime { get; set; }  // e.g. 08:00

    public TimeOnly EndTime { get; set; }    // e.g. 10:00
    
    [MaxLength(200)]
    public string? Location { get; set; }

    // Fixed hours – cannot be changed after creation
    [Range(1, 24)]
    public int Hours { get; set; }

    // Organizer / Instructor who created it
    [Required]
    public string CreatedByUserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<VolunteerSignup> Signups { get; set; } = new List<VolunteerSignup>();

    //Approval
    public bool IsApproved { get; set; }

    public string? ApprovedByUserId { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public bool IsDeletionRequested { get; set; }
    public DateTime? DeletionRequestedAt { get; set; }
    public string? DeletionRequestedByUserId { get; set; }

}