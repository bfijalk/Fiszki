using Fiszki.Components;
using Fiszki.Database;
using Fiszki.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization; // added
using Fiszki.Services.Services; // custom provider

var builder = WebApplication.CreateBuilder(args);

// Add HTTP context accessor for pages
builder.Services.AddHttpContextAccessor();

// Add Razor pages and server components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add MudBlazor services
builder.Services.AddMudServices();

// (Optional cookie auth left; not used by custom provider but can stay for future)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

// Authorization services needed for components
builder.Services.AddAuthorizationCore(); // per docs for custom auth state provider
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// Minimal direct DbContext registration (no helper extensions)
builder.Services.AddDbContext<FiszkiDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("FiszkiDatabase")));

// Register domain services
builder.Services.AddFiszkiServices(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Cookie auth middleware (harmless even if unused by custom provider)
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// --- Simple database connectivity check (synchronous) ---
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FiszkiDbContext>();
    var canConnect = db.Database.CanConnect();
    Console.WriteLine(canConnect
        ? "[Startup] Database connection successful."
        : "[Startup] Database connection FAILED.");
}
catch (Exception ex)
{
    Console.WriteLine($"[Startup] Database connection FAILED with exception: {ex.Message}");
}
// -------------------------------------------------------

app.Run();
