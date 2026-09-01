using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using AuthService.Controllers;
using AuthService.DTOs;
using AuthService.Helpers;
using AuthService.Options;
using AuthService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AuthService.Tests.Controllers;

[TestClass]
public class AuthControllerTests
{
    [TestMethod]
    public async Task Login_SetsRefreshCookie_And_ReturnsOk()
    {
        var mockAuthService = new Mock<IAuthService>(MockBehavior.Strict);
        var jwtOptions = Options.Create(new JwtOptions
        {
            RefreshCookieName = "ltms_refresh_token",
            RefreshTokenExpiryDays = 7
        });

        var controller = new AuthController(mockAuthService.Object, jwtOptions)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.HttpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");

        var userId = Guid.NewGuid();
        var loginResponse = new LoginResponseDto
        {
            AccessToken = "test-access-token",
            ExpiresAtUtc = DateTime.UtcNow.AddHours(1),
            User = new UserSummaryDto
            {
                Id = userId,
                EmployeeId = "E001",
                FullName = "Test User",
                Email = "test.user@example.com",
                Role = "Employee"
            }
        };

        mockAuthService
            .Setup(x => x.LoginAsync(It.IsAny<LoginRequestDto>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Response: loginResponse, RefreshToken: "raw-refresh-token"));

        var result = await controller.Login(new LoginRequestDto
        {
            EmployeeId = "E001",
            Password = "pass"
        }, CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        Assert.IsInstanceOfType(ok.Value, typeof(ApiResponse<LoginResponseDto>));

        var api = (ApiResponse<LoginResponseDto>)ok.Value!;
        Assert.IsTrue(api.Success);
        Assert.AreEqual("Login successful.", api.Message);
        Assert.IsNotNull(api.Data);
        Assert.AreEqual("test-access-token", api.Data!.AccessToken);

        var setCookieHeader = controller.HttpContext.Response.Headers["Set-Cookie"].ToString();
        StringAssert.Contains(setCookieHeader, "ltms_refresh_token=");
        StringAssert.Contains(setCookieHeader.ToLowerInvariant(), "httponly");
        StringAssert.Contains(setCookieHeader.ToLowerInvariant(), "secure");
        StringAssert.Contains(setCookieHeader.ToLowerInvariant(), "samesite=strict");

        mockAuthService.VerifyAll();
    }

    [TestMethod]
    public async Task Logout_DeletesRefreshCookie_And_ReturnsOk()
    {
        var userId = Guid.NewGuid();

        var mockAuthService = new Mock<IAuthService>(MockBehavior.Strict);
        mockAuthService
            .Setup(x => x.LogoutAsync(userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var jwtOptions = Options.Create(new JwtOptions
        {
            RefreshCookieName = "ltms_refresh_token",
            RefreshTokenExpiryDays = 7
        });

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())
            }, authenticationType: "test"));

        var controller = new AuthController(mockAuthService.Object, jwtOptions)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };

        var result = await controller.Logout(CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        Assert.IsInstanceOfType(ok.Value, typeof(ApiResponse<object>));

        var api = (ApiResponse<object>)ok.Value!;
        Assert.IsTrue(api.Success);
        Assert.AreEqual("Logout successful.", api.Message);

        var setCookieHeader = controller.HttpContext.Response.Headers["Set-Cookie"].ToString().ToLowerInvariant();
        StringAssert.Contains(setCookieHeader, "ltms_refresh_token=");
        StringAssert.Contains(setCookieHeader, "httponly");
        StringAssert.Contains(setCookieHeader, "secure");
        StringAssert.Contains(setCookieHeader, "samesite=strict");
        StringAssert.Contains(setCookieHeader, "expires=");

        mockAuthService.VerifyAll();
    }
}

