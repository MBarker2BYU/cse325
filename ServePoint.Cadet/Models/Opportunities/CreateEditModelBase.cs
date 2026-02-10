using System.ComponentModel.DataAnnotations;

namespace ServePoint.Cadet.Models.Opportunities;

public abstract class CreateEditModelBase
{
    [Required, MaxLength(100)]
    public string Title { get; set; } = "";

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime Date { get; set; } = DateTime.Today;

    [Range(1, 24)]
    public int Hours { get; set; } = 1;

    [Required, MaxLength(120)]
    public string ContactName { get; set; } = "";

    [EmailAddress, MaxLength(200)]
    public string? ContactEmail { get; set; }

    [Phone, MaxLength(30)]
    public string? ContactPhone { get; set; }

    [Required, MaxLength(200)]
    public string Street1 { get; set; } = "";

    [MaxLength(200)]
    public string? Street2 { get; set; }

    [Required, MaxLength(100)]
    public string City { get; set; } = "";

    [Required, MaxLength(2)]
    public string State { get; set; } = "FL";

    [Required, MaxLength(10)]
    public string PostalCode { get; set; } = "";
}