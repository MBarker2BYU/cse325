using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServePoint.Cadet.Components;
using ServePoint.Cadet.Components.Account;
using ServePoint.Cadet.Data;
using ServePoint.Cadet.Data.Initialization;
using ServePoint.Cadet.Data.Services;
using System.Data.Common;

var builder = WebApplication.CreateBuilder(args);

// Razor / Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Identity (Blazor)
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
        options.UseSqlite(connectionString);
    else
        options.UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
});


// Identity (ONE TIME)
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Email (noop)
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//Services
builder.Services.AddScoped<UserManagementService>();
builder.Services.AddScoped<OpportunityManagementService>();
builder.Services.AddScoped<DashboardService>();

var app = builder.Build();

// --- DB PREFLIGHT (provider-aware) ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Detect provider without running migrations
    var provider = db.Database.ProviderName ?? "(unknown)";
    Console.WriteLine($"DB Provider: {provider}");

    try
    {
        // This is ADO.NET-level connectivity (EF isn't doing any schema work here)
        await using DbConnection conn = db.Database.GetDbConnection();
        Console.WriteLine($"DB Connection (DataSource): {conn.DataSource}");

        await conn.OpenAsync();

        // Tiny query to prove the connection works
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = db.Database.IsSqlite()
            ? "SELECT sqlite_version();"
            : "SELECT version();";   // Postgres / SQL Server etc. can handle version()-like queries; Postgres definitely can

        var result = await cmd.ExecuteScalarAsync();
        Console.WriteLine($"Preflight OK: {result}");

        await using var accessCmd = conn.CreateCommand();
        accessCmd.CommandText = "CREATE TABLE IF NOT EXISTS __ddl_test(id int); DROP TABLE __ddl_test;";
        await cmd.ExecuteNonQueryAsync();
        Console.WriteLine("DDL OK");
        
        await conn.CloseAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Preflight FAILED:");
        Console.WriteLine(ex.ToString());
        Environment.Exit(1); // fail fast so Render shows the real reason
    }
}
// --- end preflight ---

// Pipeline
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

// Routing
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    Console.WriteLine(db.Database.ProviderName);
    Console.WriteLine(db.Database.GetDbConnection().ConnectionString);
}


await DatabaseInitializer.EnsureHealthyAsync(app.Services, app.Environment);

app.Run();