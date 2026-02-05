using System.ComponentModel.DataAnnotations;

namespace ServePoint.Cadet.Models;

public class CreateModel
{
    [Required, MaxLength(100)]
    public string Title { get; set; } = "";

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public DateTime Date { get; set; } = DateTime.Today;

    [Range(1, 24)]
    public int Hours { get; set; } = 2;

    [MaxLength(200)]
    public string? Location { get; set; }
}