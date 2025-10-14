using Fiszki.Components;
using Fiszki.Database;
using Fiszki.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Minimal direct DbContext registration (no helper extensions)
builder.Services.AddDbContext<FiszkiDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("FiszkiDatabase")));

// Register domain services
builder.Services.AddFiszkiServices();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

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
