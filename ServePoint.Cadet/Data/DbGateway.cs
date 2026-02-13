using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace ServePoint.Cadet.Data;

public sealed class DbGateway(IDbContextFactory<ApplicationDbContext> factory)
{
    // Optional: stops double-fire loads per key (Blazor render + event, etc.)
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> m_Gates = new();

    public async Task<T> ExecuteAsync<T>(
        Func<ApplicationDbContext, Task<T>> work,
        CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await work(db);
    }

    public async Task ExecuteAsync(
        Func<ApplicationDbContext, Task> work,
        CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        await work(db);
    }

    // Single-flight (optional, but helps with Blazor double-invokes)
    public async Task<T> ExecuteGatedAsync<T>(
        string gateKey,
        Func<ApplicationDbContext, Task<T>> work,
        CancellationToken ct = default)
    {
        var gate = m_Gates.GetOrAdd(gateKey, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(ct);
        try { return await ExecuteAsync(work, ct); }
        finally { gate.Release(); }
    }

    public async Task ExecuteGatedAsync(
        string gateKey,
        Func<ApplicationDbContext, Task> work,
        CancellationToken ct = default)
    {
        var gate = m_Gates.GetOrAdd(gateKey, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(ct);
        try { await ExecuteAsync(work, ct); }
        finally { gate.Release(); }
    }

    // Postgres helpers
    public static DateTime UtcNow() => DateTime.UtcNow;
    public static DateTime UtcMidnight(DateTime date) => DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
}
