using Microsoft.EntityFrameworkCore;
using ServePoint.Cadet.Data;
using ServePoint.Cadet.Models.Entities;

namespace ServePoint.Cadet.Data.Services;

public sealed class UserCardService(IDbContextFactory<ApplicationDbContext> dbFactory)
{
    public async Task<ServiceEmblem> GetEmblemForUserAsync(string userId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var record = await db.UserEmblems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        return record?.Emblem ?? ServiceEmblem.None;
    }
}