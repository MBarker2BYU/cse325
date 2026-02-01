using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServePoint.Cadet.Components;
using ServePoint.Cadet.Components.Account;
using ServePoint.Cadet.Data;

var builder = WebApplication.CreateBuilder(args);

// Connection string
var connectionString = builder.Configuration.GetConnectionString("ServePointCadetContext")
    ?? throw new InvalidOperationException("Connection string 'ServePointCadetContext' not found.");

builder.Services.AddDbContext<ServePointCadetContext>(options =>
    options.UseSqlite(connectionString));

// Razor components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// Authentication & Identity (this registers schemes + roles)
builder.Services.AddIdentity<ServePointCadetUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ServePointCadetContext>()
.AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ServePointCadetUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

// HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Authentication & Authorization middleware (order matters)
app.UseAuthentication();
app.UseAuthorization();

// Redirect 403 → AccessDenied page
app.UseStatusCodePagesWithRedirects("/Account/AccessDenied?statusCode={0}");

// Map components and Identity endpoints
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

// Seed roles on startup (only if missing)
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "User", "Organizer", "Admin", "Instructor" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

app.MapPost("/Account/Logout", async (SignInManager<ServePointCadetUser> signInManager, HttpContext context) =>
{
    await signInManager.SignOutAsync();
    context.Response.Cookies.Delete(".AspNetCore.Identity.Application");
    return Results.Redirect("/", permanent: false);
}).RequireAuthorization();

app.Run();