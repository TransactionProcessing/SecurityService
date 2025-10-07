using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using SecurityService.BusinessLogic.RequestHandlers;
using Shared.Results;
using Shouldly;
using Xunit;

public class UserRequestHandlerTests
{
    [Fact]
    public void GenerateRandomPassword_WithDefaultOptions_ReturnsValidPassword()
    {
        // Act
        var result = UserRequestHandler.GenerateRandomPassword();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var password = result.Data;
        password.ShouldNotBeNull();
        password.Length.ShouldBeGreaterThanOrEqualTo(8);
        password.Count(char.IsUpper).ShouldBeGreaterThan(0);
        password.Count(char.IsLower).ShouldBeGreaterThan(0);
        password.Count(char.IsDigit).ShouldBeGreaterThan(0);
        password.Count(c => "!@$?_-".Contains(c)).ShouldBeGreaterThan(0);
        password.Distinct().Count().ShouldBeGreaterThanOrEqualTo(4);
    }

    [Fact]
    public void GenerateRandomPassword_WithCustomOptions_ReturnsValidPassword()
    {
        var options = new PasswordOptions
        {
            RequiredLength = 12,
            RequiredUniqueChars = 6,
            RequireDigit = true,
            RequireLowercase = true,
            RequireNonAlphanumeric = true,
            RequireUppercase = true
        };

        var result = UserRequestHandler.GenerateRandomPassword(options);

        result.IsSuccess.ShouldBeTrue();
        var password = result.Data;
        password.ShouldNotBeNull();
        password.Length.ShouldBeGreaterThanOrEqualTo(12);
        password.Distinct().Count().ShouldBeGreaterThanOrEqualTo(6);
        password.Count(char.IsUpper).ShouldBeGreaterThan(0);
        password.Count(char.IsLower).ShouldBeGreaterThan(0);
        password.Count(char.IsDigit).ShouldBeGreaterThan(0);
        password.Count(c => "!@$?_-".Contains(c)).ShouldBeGreaterThan(0);
    }

    [Fact]
    public void GenerateRandomPassword_TooManyUniqueChars_ReturnsFailure()
    {
        var options = new PasswordOptions
        {
            RequiredLength = 10,
            RequiredUniqueChars = 100, // Exceeds available unique chars
            RequireDigit = true,
            RequireLowercase = true,
            RequireNonAlphanumeric = true,
            RequireUppercase = true
        };

        var result = UserRequestHandler.GenerateRandomPassword(options);

        result.IsFailed.ShouldBeTrue();
        result.Data.ShouldBeNull();
        result.Errors.Count.ShouldBeGreaterThan(0);
        result.Errors.First().ShouldContain("RequiredUniqueChars");
    }

    [Fact]
    public void GenerateRandomPassword_NoCategories_FallbackToLowerAndDigits()
    {
        var options = new PasswordOptions
        {
            RequiredLength = 8,
            RequiredUniqueChars = 4,
            RequireDigit = false,
            RequireLowercase = false,
            RequireNonAlphanumeric = false,
            RequireUppercase = false
        };

        var result = UserRequestHandler.GenerateRandomPassword(options);

        result.IsSuccess.ShouldBeTrue();
        var password = result.Data;
        password.ShouldNotBeNull();
        password.Length.ShouldBeGreaterThanOrEqualTo(8);
        password.All(c => "abcdefghijkmnopqrstuvwxyz0123456789".Contains(c)).ShouldBeTrue();
    }
}