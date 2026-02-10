using System.ComponentModel.DataAnnotations;

namespace ServePoint.Cadet.Models.Entities;

public class Contact
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress, MaxLength(200)]
    public string? Email { get; set; }

    [Phone, MaxLength(30)]
    public string? Phone { get; set; }

    public int? AddressId { get; set; }
    public Address? Address { get; set; }
}