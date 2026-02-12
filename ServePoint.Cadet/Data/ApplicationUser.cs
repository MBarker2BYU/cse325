using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ServePoint.Cadet.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    [MaxLength(50)]
    public string? FirstName { get; set; }

    [MaxLength(50)]
    public string? MiddleName { get; set; }   // optional

    [MaxLength(50)]
    public string? LastName { get; set; }

    public string DisplayName
    {
        get
        {
            var first = (FirstName ?? "").Trim();
            var middle = (MiddleName ?? "").Trim();
            var last = (LastName ?? "").Trim();

            if (string.IsNullOrWhiteSpace(first) && string.IsNullOrWhiteSpace(last))
                return Email ?? UserName ?? "Unknown";

            var middleInitial =
                string.IsNullOrWhiteSpace(middle) ? "" : $" {char.ToUpperInvariant(middle[0])}.";

            return $"{last.ToUpperInvariant()}, {first}{middleInitial}";
        }
    }
}

