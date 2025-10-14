using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Fiszki.Services.Commands;
using Fiszki.Services.Interfaces;
using Fiszki.Services.Services;

namespace Fiszki.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddFiszkiServices(this IServiceCollection services)
    {
        // Validators
        services.AddScoped<IValidator<RegisterUserCommand>, Validation.RegisterUserCommandValidator>();
        services.AddScoped<IValidator<LoginCommand>, Validation.LoginCommandValidator>();
        services.AddScoped<IValidator<CreateFlashcardCommand>, Validation.CreateFlashcardCommandValidator>();
        services.AddScoped<IValidator<UpdateFlashcardCommand>, Validation.UpdateFlashcardCommandValidator>();

        // Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IFlashcardService, FlashcardService>();

        return services;
    }
}
