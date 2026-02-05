using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServePoint.Cadet.Models.Entities;

namespace ServePoint.Cadet.Data;

public class ServePointCadetContext(DbContextOptions<ServePointCadetContext> options)
    : IdentityDbContext<ServePointCadetUser>(options)
{
    public DbSet<VolunteerOpportunity> VolunteerOpportunities => Set<VolunteerOpportunity>();
    public DbSet<VolunteerSignup> VolunteerSignups => Set<VolunteerSignup>();

    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Address> Addresses => Set<Address>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Prevent duplicate signups for the same opportunity
        builder.Entity<VolunteerSignup>()
            .HasIndex(v => new { v.VolunteerOpportunityId, v.UserId })
            .IsUnique();

        // VolunteerOpportunity -> Contact (required)
        builder.Entity<VolunteerOpportunity>()
            .HasOne(o => o.Contact)
            .WithMany()
            .HasForeignKey(o => o.ContactId)
            .OnDelete(DeleteBehavior.Restrict);

        // Contact -> Address (optional)
        builder.Entity<Contact>()
            .HasOne(c => c.Address)
            .WithMany()
            .HasForeignKey(c => c.AddressId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}