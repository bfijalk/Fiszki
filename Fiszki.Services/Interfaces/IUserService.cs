using Fiszki.Services.Commands;
using Fiszki.Services.Models;

namespace Fiszki.Services.Interfaces;

public interface IUserService
{
    Task<UserDto> RegisterAsync(RegisterUserCommand command, CancellationToken ct = default);
    Task<UserDto> LoginAsync(LoginCommand command, CancellationToken ct = default);
    Task<UserDto> GetByIdAsync(Guid userId, CancellationToken ct = default);
}

