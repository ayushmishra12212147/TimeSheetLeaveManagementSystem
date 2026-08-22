using System.Security.Cryptography;
using System.Text;
using AuthService.Data;
using AuthService.DTOs;
using AuthService.Messaging;
using AuthService.Options;
using AuthService.Services;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AuthService.Tests.Services;

[TestClass]
public class AuthServiceTests
{
    [TestMethod]
    public async Task LoginAsync_ValidCredentials_CreatesRefreshToken_And_ReturnsAccessToken()
    {
        var userId = Guid.NewGuid();
        var authDbOptions = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var employeeDbOptions = new DbContextOptionsBuilder<EmployeeIdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var authDb = new AuthDbContext(authDbOptions);
        await using var employeeDb = new EmployeeIdentityDbContext(employeeDbOptions);

        var user = new EmployeeUser
        {
            Id = userId,
            EmployeeId = "E001",
            FullName = "Test User",
            Email = "test.user@example.com",
            Role = "Employee",
            Password = BCrypt.Net.BCrypt.HashPassword("pass"),
            IsFirstLogin = false,
            MustResetPassword = false,
            IsProfileComplete = true
        };
        employeeDb.Users.Add(user);
        await employeeDb.SaveChangesAsync();

        var jwtOptions = Options.Create(new JwtOptions
        {
            Secret = "TEST-SECRET-KEY-32-BYTES-OR-MORE-123456",
            Issuer = "LTMS.AuthService",
            Audience = "LTMS.Client",
            AccessTokenExpiryHours = 8,
            RefreshTokenExpiryDays = 7
        });

        var jwtTokenService = new JwtTokenService(jwtOptions);
        var publisher = new Mock<IRabbitMQPublisher>(MockBehavior.Strict);

        var service = new AuthService.Services.AuthService(
            authDb,
            employeeDb,
            jwtTokenService,
            publisher.Object,
            jwtOptions);

        var (response, refreshToken) = await service.LoginAsync(new LoginRequestDto
        {
            EmployeeId = "e001",
            Password = "pass"
        }, ipAddress: "127.0.0.1", CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.IsFalse(string.IsNullOrWhiteSpace(response.AccessToken));
        Assert.IsFalse(string.IsNullOrWhiteSpace(refreshToken));

        var stored = await authDb.RefreshTokens.SingleAsync(x => x.UserId == userId);
        Assert.IsNull(stored.RevokedAtUtc);
        Assert.AreEqual(ComputeHash(refreshToken), stored.TokenHash);

        var attempt = await authDb.LoginAttempts.SingleAsync(x => x.UserId == userId);
        Assert.IsTrue(attempt.WasSuccessful);
        Assert.AreEqual("127.0.0.1", attempt.IpAddress);
    }

    [TestMethod]
    public async Task LogoutAsync_RevokesActiveRefreshTokens()
    {
        var userId = Guid.NewGuid();
        var authDbOptions = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var employeeDbOptions = new DbContextOptionsBuilder<EmployeeIdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var authDb = new AuthDbContext(authDbOptions);
        await using var employeeDb = new EmployeeIdentityDbContext(employeeDbOptions);

        authDb.RefreshTokens.AddRange(
            new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = ComputeHash("t1"),
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-5),
                ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
            },
            new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = ComputeHash("t2"),
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-3),
                ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
            }
        );
        await authDb.SaveChangesAsync();

        var jwtOptions = Options.Create(new JwtOptions
        {
            Secret = "TEST-SECRET-KEY-32-BYTES-OR-MORE-123456",
            Issuer = "LTMS.AuthService",
            Audience = "LTMS.Client"
        });

        var jwtTokenService = new JwtTokenService(jwtOptions);
        var publisher = new Mock<IRabbitMQPublisher>(MockBehavior.Strict);

        var service = new AuthService.Services.AuthService(
            authDb,
            employeeDb,
            jwtTokenService,
            publisher.Object,
            jwtOptions);

        await service.LogoutAsync(userId, CancellationToken.None);

        var tokens = await authDb.RefreshTokens.Where(x => x.UserId == userId).ToListAsync();
        Assert.AreEqual(2, tokens.Count);
        Assert.IsTrue(tokens.All(x => x.RevokedAtUtc != null));
    }

    private static string ComputeHash(string rawValue)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawValue));
        return Convert.ToHexString(bytes);
    }
}

