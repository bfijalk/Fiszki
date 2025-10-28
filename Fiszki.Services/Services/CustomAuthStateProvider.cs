using System.Security.Claims;
using System.Threading.Tasks;
using Fiszki.Services.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace Fiszki.Services.Services;

// Simple in-memory authentication state provider (MVP) modeled after MS docs
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

    public override Task<AuthenticationState> GetAuthenticationStateAsync() 
    {
        Console.WriteLine($"[Auth] GetAuthenticationStateAsync called. User authenticated: {_currentUser.Identity?.IsAuthenticated}");
        return Task.FromResult(new AuthenticationState(_currentUser));
    }

    public void MarkUserAsAuthenticated(UserDto user)
    {
        Console.WriteLine($"[Auth] MarkUserAsAuthenticated called for user: {user.Email}");
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        }, authenticationType: "Custom");
        _currentUser = new ClaimsPrincipal(identity);
        Console.WriteLine($"[Auth] User marked as authenticated. Claims: {string.Join(", ", _currentUser.Claims.Select(c => $"{c.Type}={c.Value}"))}");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void MarkUserAsLoggedOut()
    {
        Console.WriteLine("[Auth] MarkUserAsLoggedOut called");
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
