
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using WebApplication1;
using WebApplication1.Models;
using WebApplication1.Repositories.Contracts;
using Xunit;

namespace TestProject;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _repository = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var settings = new Dictionary<string, string?>
        {
            ["AppSettings:Token"] = new string('A', 64),
            ["AppSettings:Issuer"] = "TestIssuer",
            ["AppSettings:Audience"] = "TestAudience"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        _service = new AuthService(
            _repository.Object,
            configuration);
    }

    // REGISTER TESTS

    [Theory]
    [InlineData("", "Username cannot be empty")]
    [InlineData("ab", "Username must be at least 3 characters long")]
    [InlineData("john123", "Username can contain letters only")]
    [InlineData("abcdefghijklmnopqrstu",
        "Username must be maximum 20 characters long")]
    public async Task RegisterAsync_ShouldRejectInvalidUsername(
        string username,
        string expectedError)
    {
        // Arrange
        var request = new UserDto
        {
            Username = username,
            Password = "ValidPass123!"
        };

        // Act
        var (user, error) = await _service.RegisterAsync(request);

        // Assert
        user.Should().BeNull();
        error.Should().Be(expectedError);

        _repository.Verify(
            r => r.AddAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("", "Password cannot be empty")]
    [InlineData("Ab1!", "Password must be at least 8 characters long")]
    [InlineData("password123!",
        "Password must contain at least one uppercase letter")]
    [InlineData("PASSWORD123!",
        "Password must contain at least one lowercase letter")]
    [InlineData("Password!",
        "Password must contain at least one digit")]
    [InlineData("Password123",
        "Password must contain at least one special character")]
    public async Task RegisterAsync_ShouldRejectInvalidPassword(
        string password,
        string expectedError)
    {
        // Arrange
        var request = new UserDto
        {
            Username = "John",
            Password = password
        };

        // Act
        var (user, error) = await _service.RegisterAsync(request);

        // Assert
        user.Should().BeNull();
        error.Should().Be(expectedError);

        _repository.Verify(
            r => r.AddAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldRejectDuplicateUsername()
    {
        // Arrange
        _repository
            .Setup(r => r.UsernameExistsAsync(
                "John",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new UserDto
        {
            Username = "John",
            Password = "ValidPass123!"
        };

        // Act
        var (user, error) = await _service.RegisterAsync(request);

        // Assert
        user.Should().BeNull();
        error.Should().Be("User already exists");

        _repository.Verify(
            r => r.AddAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUserWithHashedPassword()
    {
        // Arrange
        var request = new UserDto
        {
            Username = "John",
            Password = "ValidPass123!"
        };

        User? savedUser = null;

        _repository
            .Setup(r => r.UsernameExistsAsync(
                "John",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repository
            .Setup(r => r.AddAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((createdUser, ct) =>
                savedUser = createdUser)
            .Returns(Task.CompletedTask);

        // Act
        var (user, error) = await _service.RegisterAsync(request);

        // Assert
        error.Should().BeNull();
        user.Should().NotBeNull();

        savedUser.Should().NotBeNull();
        savedUser!.Username.Should().Be("John");
        savedUser.Role.Should().Be("User");

        savedUser.PasswordHash.Should().NotBe(request.Password);

        var verification = new PasswordHasher<User>()
            .VerifyHashedPassword(
                savedUser,
                savedUser.PasswordHash,
                request.Password);

        verification.Should().NotBe(PasswordVerificationResult.Failed);

        _repository.Verify(
            r => r.AddAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // LOGIN TESTS

    [Fact]
    public async Task LoginAsync_ShouldFail_WhenUserDoesNotExist()
    {
        // Arrange
        _repository
            .Setup(r => r.GetByUsernameAsync(
                "Unknown",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var request = new UserDto
        {
            Username = "Unknown",
            Password = "Password123!"
        };

        // Act
        var (tokens, error) = await _service.LoginAsync(request);

        // Assert
        tokens.Should().BeNull();
        error.Should().Be("Invalid username or password");

        _repository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldFail_WhenPasswordIsWrong()
    {
        // Arrange
        var user = CreateUser();

        _repository
            .Setup(r => r.GetByUsernameAsync(
                "John",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var request = new UserDto
        {
            Username = "John",
            Password = "WrongPassword123!"
        };

        // Act
        var (tokens, error) = await _service.LoginAsync(request);

        // Assert
        tokens.Should().BeNull();
        error.Should().Be("Invalid username or password");

        _repository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        // Arrange
        var user = CreateUser();

        _repository
            .Setup(r => r.GetByUsernameAsync(
                "John",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var request = new UserDto
        {
            Username = "John",
            Password = "ValidPass123!"
        };

        // Act
        var (tokens, error) = await _service.LoginAsync(request);

        // Assert
        error.Should().BeNull();
        tokens.Should().NotBeNull();

        tokens!.AccessToken.Should().NotBeNullOrWhiteSpace();
        tokens.RefreshToken.Should().NotBeNullOrWhiteSpace();

        user.RefreshToken.Should().Be(tokens.RefreshToken);
        user.RefreshTokenExpiryTime.Should()
            .BeAfter(DateTime.UtcNow);

        _repository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // REFRESH TOKEN TESTS

    [Fact]
    public async Task RefreshTokensAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        _repository
            .Setup(r => r.GetByIdAsync(
                99,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var request = new RefreshTokenRequestDto
        {
            UserId = 99,
            RefreshToken = "test-token"
        };

        // Act
        var result = await _service.RefreshTokensAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshTokensAsync_ShouldRejectInvalidRefreshToken()
    {
        // Arrange
        var user = CreateUser();

        user.RefreshToken = "valid-token";
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);

        _repository
            .Setup(r => r.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var request = new RefreshTokenRequestDto
        {
            UserId = user.Id,
            RefreshToken = "wrong-token"
        };

        // Act
        var result = await _service.RefreshTokensAsync(request);

        // Assert
        result.Should().BeNull();

        _repository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshTokensAsync_ShouldRejectExpiredToken()
    {
        // Arrange
        var user = CreateUser();

        user.RefreshToken = "old-token";
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1);

        _repository
            .Setup(r => r.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var request = new RefreshTokenRequestDto
        {
            UserId = user.Id,
            RefreshToken = "old-token"
        };

        // Act
        var result = await _service.RefreshTokensAsync(request);

        // Assert
        result.Should().BeNull();

        _repository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshTokensAsync_ShouldGenerateNewTokens()
    {
        // Arrange
        var user = CreateUser();

        user.RefreshToken = "old-token";
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);

        _repository
            .Setup(r => r.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var request = new RefreshTokenRequestDto
        {
            UserId = user.Id,
            RefreshToken = "old-token"
        };

        // Act
        var result = await _service.RefreshTokensAsync(request);

        // Assert
        result.Should().NotBeNull();

        result!.RefreshToken.Should().NotBe("old-token");
        result.AccessToken.Should().NotBeNullOrWhiteSpace();

        user.RefreshTokenExpiryTime.Should()
            .BeAfter(DateTime.UtcNow);

        _repository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // HELPERS

    private static User CreateUser()
    {
        var user = new User
        {
            Id = 1,
            Username = "John",
            Role = "User"
        };

        user.PasswordHash = new PasswordHasher<User>()
            .HashPassword(user, "ValidPass123!");

        return user;
    }
}
