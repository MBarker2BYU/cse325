using System.ComponentModel.DataAnnotations;

namespace ServePoint.Cadet.Models;

public class EditModel
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Title { get; set; } = "";

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public DateTime Date { get; set; } = DateTime.Today;

    [Range(1, 24)]
    public int Hours { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }
}