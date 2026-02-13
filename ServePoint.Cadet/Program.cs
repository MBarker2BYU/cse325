using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics; // ✅ ADD THIS
using ServePoint.Cadet.Components;
using ServePoint.Cadet.Components.Account;
using ServePoint.Cadet.Data;
using ServePoint.Cadet.Data.Initialization;
using ServePoint.Cadet.Data.Services;
using System.Data.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.UseSqlite(connectionString);
    }
    else
    {
        options.UseNpgsql(connectionString);

        // ✅ Render: don't crash on this warning during MigrateAsync()
        options.ConfigureWarnings(w =>
            w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }
});

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true; // consider false for testing
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddScoped<UserManagementService>();
builder.Services.AddScoped<OpportunityManagementService>();
builder.Services.AddScoped<DashboardService>();

var app = builder.Build();

//
// --- DB PREFLIGHT (safe) ---
//
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    Console.WriteLine($"DB Provider: {db.Database.ProviderName}");
    await using DbConnection conn = db.Database.GetDbConnection();
    Console.WriteLine($"DB DataSource: {conn.DataSource}");

    await conn.OpenAsync();

    // Basic query proves connectivity
    await using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = db.Database.IsSqlite() ? "SELECT sqlite_version();" : "SELECT version();";
        var result = await cmd.ExecuteScalarAsync();
        Console.WriteLine($"Preflight OK: {result}");
    }

    // Show pending migrations (proves EF can see them)
    var pending = await db.Database.GetPendingMigrationsAsync();
    Console.WriteLine("Pending migrations: " + string.Join(", ", pending));

    // Apply migrations (creates AspNetRoles etc.)
    await db.Database.MigrateAsync();
    Console.WriteLine("MigrateAsync completed.");

    await conn.CloseAsync();
}
catch (Exception ex)
{
    Console.WriteLine("Preflight FAILED:");
    Console.WriteLine(ex);
    Environment.Exit(1);
}
// --- end preflight ---

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

await DatabaseInitializer.EnsureHealthyAsync(app.Services, app.Environment);

app.Run();
