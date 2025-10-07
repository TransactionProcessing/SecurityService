using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using SecurityService.BusinessLogic.RequestHandlers;
using Shouldly;
using SimpleResults;
using Xunit;

namespace SecurityService.UnitTests;

public class PasswordGeneratorTests
{
    [Fact]
    public void GenerateRandomPassword_WithDefaultOptions_ReturnsValidPassword()
    {
        // Act
        Result<String> result = PasswordGenerator.GenerateRandomPassword();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        String password = result.Data;
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
        PasswordOptions options = new PasswordOptions
        {
            RequiredLength = 12,
            RequiredUniqueChars = 6,
            RequireDigit = true,
            RequireLowercase = true,
            RequireNonAlphanumeric = true,
            RequireUppercase = true
        };

        Result<String> result = PasswordGenerator.GenerateRandomPassword(options);

        result.IsSuccess.ShouldBeTrue();
        String password = result.Data;
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
        PasswordOptions options = new PasswordOptions
        {
            RequiredLength = 10,
            RequiredUniqueChars = 100, // Exceeds available unique chars
            RequireDigit = true,
            RequireLowercase = true,
            RequireNonAlphanumeric = true,
            RequireUppercase = true
        };

        Result<String> result = PasswordGenerator.GenerateRandomPassword(options);

        result.IsFailed.ShouldBeTrue();
        result.Data.ShouldBeNull();
        result.Message.ShouldNotBeEmpty();
        result.Message.ShouldContain("is greater than available unique characters");
    }

    [Fact]
    public void GenerateRandomPassword_NoCategories_FallbackToLowerAndDigits()
    {
        PasswordOptions options = new PasswordOptions
        {
            RequiredLength = 8,
            RequiredUniqueChars = 4,
            RequireDigit = false,
            RequireLowercase = false,
            RequireNonAlphanumeric = false,
            RequireUppercase = false
        };

        Result<String> result = PasswordGenerator.GenerateRandomPassword(options);

        result.IsSuccess.ShouldBeTrue();
        String password = result.Data;
        password.ShouldNotBeNull();
        password.Length.ShouldBeGreaterThanOrEqualTo(8);
        password.All(c => "abcdefghijkmnopqrstuvwxyz0123456789".Contains(c)).ShouldBeTrue();
    }
}