using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Fiszki.Database.Entities;
using Fiszki.Services.Commands;
using Fiszki.Services.Exceptions;
using Fiszki.Services.Services;
using Fiszki.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Fiszki.Tests.Services;

public class UserServiceTests : IDisposable
{
    private readonly Mock<IValidator<RegisterUserCommand>> _registerValidatorMock;
    private readonly Mock<IValidator<LoginCommand>> _loginValidatorMock;
    private readonly UserService _userService;
    private readonly Database.FiszkiDbContext _dbContext;
    private readonly Fixture _fixture;

    public UserServiceTests()
    {
        _registerValidatorMock = new Mock<IValidator<RegisterUserCommand>>();
        _loginValidatorMock = new Mock<IValidator<LoginCommand>>();
        _dbContext = TestDbContextFactory.CreateInMemoryContext();
        _userService = new UserService(_dbContext, _registerValidatorMock.Object, _loginValidatorMock.Object);
        _fixture = new Fixture();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Theory]
    [AutoData]
    public async Task RegisterAsync_WithValidCommand_ShouldCreateUserSuccessfully(
        string email, 
        string password)
    {
        // Arrange
        var command = new RegisterUserCommand(email, password);
        _registerValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _userService.RegisterAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(email);
        result.IsActive.Should().BeTrue();
        result.Role.Should().Be("Basic");
        result.TotalCardsGenerated.Should().Be(0);
        result.TotalCardsAccepted.Should().Be(0);

        // Verify user was saved to database
        var userInDb = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        userInDb.Should().NotBeNull();
        userInDb!.Email.Should().Be(email);
        
        // Verify password was hashed
        BCrypt.Net.BCrypt.Verify(password, userInDb.PasswordHash).Should().BeTrue();
    }

    [Theory]
    [AutoData]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowConflictException(
        string email, 
        string password)
    {
        // Arrange
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("existing_password"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync();

        var command = new RegisterUserCommand(email, password);
        _registerValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act & Assert
        var act = () => _userService.RegisterAsync(command);
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage($"User with email {email} already exists");
    }

    [Theory]
    [AutoData]
    public async Task RegisterAsync_WithInvalidCommand_ShouldThrowValidationException(
        string email, 
        string password)
    {
        // Arrange
        var command = new RegisterUserCommand(email, password);
        _registerValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<RegisterUserCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new [] { new ValidationFailure("Email", "Validation failed") }));

        // Act & Assert
        var act = () => _userService.RegisterAsync(command);
        await act.Should().ThrowAsync<ValidationException>()
            .Where(e => e.Message.StartsWith("Validation failed"));
    }

    [Theory]
    [AutoData]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnUserDto(
        string email, 
        string password)
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = hashedPassword,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var command = new LoginCommand(email, password);
        _loginValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _userService.LoginAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Email.Should().Be(email);
        result.IsActive.Should().BeTrue();
    }

    [Theory]
    [AutoData]
    public async Task LoginAsync_WithInvalidEmail_ShouldThrowUnauthorizedException(
        string email, 
        string password)
    {
        // Arrange
        var command = new LoginCommand(email, password);
        _loginValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act & Assert
        var act = () => _userService.LoginAsync(command);
        await act.Should().ThrowAsync<UnauthorizedDomainException>()
            .WithMessage("Invalid credentials");
    }

    [Theory]
    [AutoData]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowUnauthorizedException(
        string email, 
        string correctPassword, 
        string wrongPassword)
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = hashedPassword,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var command = new LoginCommand(email, wrongPassword);
        _loginValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act & Assert
        var act = () => _userService.LoginAsync(command);
        await act.Should().ThrowAsync<UnauthorizedDomainException>()
            .WithMessage("Invalid credentials");
    }

    [Theory]
    [AutoData]
    public async Task LoginAsync_WithInactiveUser_ShouldThrowUnauthorizedException(
        string email, 
        string password)
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = hashedPassword,
            IsActive = false, // Inactive user
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var command = new LoginCommand(email, password);
        _loginValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act & Assert
        var act = () => _userService.LoginAsync(command);
        await act.Should().ThrowAsync<UnauthorizedDomainException>()
            .WithMessage("Account inactive");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingUser_ShouldReturnUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _userService.GetByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Email.Should().Be("test@example.com");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentUser_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        var act = () => _userService.GetByIdAsync(userId);
        await act.Should().ThrowAsync<DomainNotFoundException>()
            .WithMessage("User not found");
    }
}
