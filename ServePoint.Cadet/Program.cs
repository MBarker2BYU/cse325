
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServePoint.Cadet.Components;
using ServePoint.Cadet.Components.Account;
using ServePoint.Cadet.Data;
using ServePoint.Cadet.Data.Initialization;
using ServePoint.Cadet.Data.Services;
using ServePoint.Cadet.Diagnostics;
using ServePoint.Cadet.Reports.Services;
using System.Text;

using System.Security.Claims;
using QuestPDF.Infrastructure;
using ServePoint.Cadet.Reports;

var builder = WebApplication.CreateBuilder(args);

// Razor / Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Identity (Blazor)
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");

// DO NOT USE IN APP EVER!!! Use DbGateway for data access.
builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseNpgsql(connectionString),
    contextLifetime: ServiceLifetime.Scoped,
    optionsLifetime: ServiceLifetime.Singleton
);

// DO NOT USE IN APP EVER!!! Use DbGateway for data access.
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Use this for Data
builder.Services.AddScoped<DbGateway>();

// Identity
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.Redirect("/Account/AccessDenied");
        return Task.CompletedTask;
    };
});


// Email (noop)
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// App Services
builder.Services.AddScoped<UserManagementService>();
builder.Services.AddScoped<OpportunityManagementService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<VolunteerHoursReportService>();


var app = builder.Build();

QuestPDF.Settings.License = LicenseType.Community;

//Production Issues
var options = new ForwardedHeadersOptions()
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};

options.KnownProxies.Clear();
options.KnownIPNetworks.Clear();

app.UseForwardedHeaders(options);

// Capture unhandled exceptions (REMOVE after fixing)
app.Use(async (ctx, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        ProductionDiagnostics.LastErrorAtUtc = DateTime.UtcNow;
        ProductionDiagnostics.LastError = ex.ToString();

        Console.WriteLine("UNHANDLED EXCEPTION:");
        Console.WriteLine(ProductionDiagnostics.LastError);

        throw;
    }
});

// Diagnostic endpoint (REMOVE after fixing)
app.MapGet("/_diag/last-error", () =>
{
    if (ProductionDiagnostics.LastError is null)
        return Results.Text("No captured exception yet.");

    var sb = new StringBuilder();
    sb.AppendLine($"UTC: {ProductionDiagnostics.LastErrorAtUtc:O}");
    sb.AppendLine(ProductionDiagnostics.LastError);
    return Results.Text(sb.ToString(), "text/plain");
});

// Reports
app.MapGet("/reports/hours/pdf",
        async (HttpContext http,
            VolunteerHoursReportService reportSvc,
            string userId,
            DateTime? from,
            DateTime? to,
            CancellationToken ct) =>
        {
            var requesterId = http.User.FindFirstValue(ClaimTypes.NameIdentifier)
                              ?? throw new InvalidOperationException("No requester id.");

            var report = await reportSvc.GetReportAsync(requesterId, userId, from, to, ct);

            var pdfBytes = ServePoint.Cadet.Reports.VolunteerHoursToPDF.Build(report,from, to);

            return Results.File(
                pdfBytes,
                "application/pdf",
                $"ServePointHours_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf");
        })
    .RequireAuthorization();


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


app.UseStatusCodePages(async context =>
{
    var http = context.HttpContext;

    if (http.Response.StatusCode == 404 &&
        HttpMethods.IsGet(http.Request.Method))
    {
        http.Response.Redirect("/not-found");
    }

    await Task.CompletedTask;
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

//app.MapAdditionalIdentityEndpoints();

// Initialization MUST be run in a scope
await using (var scope = app.Services.CreateAsyncScope())
{
    await DatabaseInitializer.EnsureHealthyAsync(scope.ServiceProvider, app.Environment);
}

app.Run();
