using System.ComponentModel.DataAnnotations;

namespace ServePoint.Cadet.Models.Entities;

public class Address
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Street1 { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Street2 { get; set; }

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, MaxLength(2)]
    public string State { get; set; } = "FL";

    [Required, MaxLength(10)]
    public string PostalCode { get; set; } = string.Empty;
}