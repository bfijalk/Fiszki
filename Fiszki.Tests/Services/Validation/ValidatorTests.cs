using FluentAssertions;
using FluentValidation.TestHelper;
using Fiszki.Services.Commands;
using Fiszki.Services.Validation;
using Xunit;

namespace Fiszki.Tests.Services.Validation;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Fact]
    public void ValidCommand_ShouldPass()
    {
        var cmd = new RegisterUserCommand("user@example.com", "Pass123");
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyEmail_ShouldFail(string email)
    {
        var cmd = new RegisterUserCommand(email, "Pass123");
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("a1")]
    public void TooShortPassword_ShouldFail(string pwd)
    {
        var cmd = new RegisterUserCommand("user@example.com", pwd);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(c => c.Password);
    }

    [Fact]
    public void PasswordWithoutDigit_ShouldFail()
    {
        var cmd = new RegisterUserCommand("user@example.com", "Password");
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(c => c.Password)
              .WithErrorMessage("Password must contain digit");
    }
}

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void ValidLogin_ShouldPass()
    {
        var cmd = new LoginCommand("user@example.com", "Abc123");
        _validator.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyEmail_ShouldFail()
    {
        var cmd = new LoginCommand("", "Abc123");
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void EmptyPassword_ShouldFail()
    {
        var cmd = new LoginCommand("user@example.com", "");
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(c => c.Password);
    }
}

public class CreateFlashcardCommandValidatorTests
{
    private readonly CreateFlashcardCommandValidator _validator = new();

    [Fact]
    public void ValidFlashcard_ShouldPass()
    {
        var cmd = new CreateFlashcardCommand("Front", "Back", new [] { "tag1" });
        _validator.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyFront_ShouldFail()
    {
        var cmd = new CreateFlashcardCommand("", "Back", null);
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(c => c.FrontContent);
    }

    [Fact]
    public void TooManyTags_ShouldFail()
    {
        var tags = Enumerable.Range(0, 11).Select(i => $"t{i}").ToList();
        var cmd = new CreateFlashcardCommand("Front", "Back", tags);
        var res = _validator.TestValidate(cmd);
        res.ShouldHaveValidationErrorFor(c => c.Tags);
    }
}

public class StartGenerationCommandValidatorTests
{
    private readonly StartGenerationCommandValidator _validator = new();

    [Fact]
    public void ValidStart_ShouldPass()
    {
        var cmd = new StartGenerationCommand { SourceText = new string('a', 60), Language = "pl", MaxCards = 10 };
        _validator.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void TooShortSource_ShouldFail()
    {
        var cmd = new StartGenerationCommand { SourceText = "short", Language = "en", MaxCards = 5 };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(c => c.SourceText);
    }

    [Fact]
    public void UnsupportedLanguage_ShouldFail()
    {
        var cmd = new StartGenerationCommand { SourceText = new string('a', 60), Language = "xx", MaxCards = 5 };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(c => c.Language);
    }

    [Fact]
    public void MaxCardsOutOfRange_ShouldFail()
    {
        var cmd = new StartGenerationCommand { SourceText = new string('a', 60), Language = "pl", MaxCards = 0 };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(c => c.MaxCards);
    }
}

