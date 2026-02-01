using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServePoint.Cadet.Data;

namespace ServePoint.Cadet.Data
{
    public class ServePointCadetContext(DbContextOptions<ServePointCadetContext> options) : IdentityDbContext<ServePointCadetUser>(options)
    {
    }
}
