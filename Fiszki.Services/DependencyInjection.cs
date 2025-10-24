using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Fiszki.Services.Commands;
using Fiszki.Services.Interfaces;
using Fiszki.Services.Services;
using Fiszki.Services.Interfaces.OpenRouter;
using Fiszki.Services.Services.OpenRouter;
using Fiszki.Services.Models.OpenRouter;

namespace Fiszki.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddFiszkiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure OpenRouter options
        services.Configure<OpenRouterOptions>(configuration.GetSection("OpenRouter"));

        // Validators
        services.AddScoped<IValidator<RegisterUserCommand>, Validation.RegisterUserCommandValidator>();
        services.AddScoped<IValidator<LoginCommand>, Validation.LoginCommandValidator>();
        services.AddScoped<IValidator<CreateFlashcardCommand>, Validation.CreateFlashcardCommandValidator>();
        services.AddScoped<IValidator<UpdateFlashcardCommand>, Validation.UpdateFlashcardCommandValidator>();
        services.AddScoped<IValidator<StartGenerationCommand>, Validation.StartGenerationCommandValidator>();
        services.AddScoped<IValidator<SaveProposalsCommand>, Validation.SaveProposalsCommandValidator>();

        // Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IFlashcardService, FlashcardService>();
        services.AddScoped<IGenerationService, GenerationService>();

        // OpenRouter Services
        services.AddHttpClient<OpenRouterChatService>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpenRouterOptions>>().Value;
            client.BaseAddress = options.BaseUri;
            client.Timeout = options.RequestTimeout;
        });
        
        services.AddScoped<IOpenRouterChatService, OpenRouterChatService>();

        return services;
    }
}
