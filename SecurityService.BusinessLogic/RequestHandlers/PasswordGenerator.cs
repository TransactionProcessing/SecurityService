using Microsoft.AspNetCore.Identity;
using SecurityService.BusinessLogic;
using Shared.Results;
using SimpleResults;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace SecurityService.BusinessLogic.RequestHandlers;

public static class PasswordGenerator
{
    public static Result<string> GenerateRandomPassword(PasswordOptions? opts = null)
    {
        opts ??= DefaultOptions();

        var categories = BuildCategories(opts);
        var result = ValidateUniqueCharRequirement(opts, categories);
        if (result.IsFailed)
            return ResultHelpers.CreateFailure(result);

        var chars = new List<char>();

        AddRequiredCategoryChars(chars, categories);
        FillRemainingChars(chars, opts, categories);
        SecureShuffle(chars);

        return Result.Success(new string(chars.ToArray()));
    }

    private static PasswordOptions DefaultOptions() => new()
    {
        RequiredLength = 8,
        RequiredUniqueChars = 4,
        RequireDigit = true,
        RequireLowercase = true,
        RequireNonAlphanumeric = true,
        RequireUppercase = true
    };

    private static List<string> BuildCategories(PasswordOptions opts)
    {
        var list = new List<string>();
        if (opts.RequireUppercase) list.Add("ABCDEFGHJKLMNOPQRSTUVWXYZ");
        if (opts.RequireLowercase) list.Add("abcdefghijkmnopqrstuvwxyz");
        if (opts.RequireDigit) list.Add("0123456789");
        if (opts.RequireNonAlphanumeric) list.Add("!@$?_-");
        if (!list.Any()) list.Add("abcdefghijkmnopqrstuvwxyz0123456789");
        return list;
    }

    private static Result ValidateUniqueCharRequirement(PasswordOptions opts, List<string> categories)
    {
        var all = string.Concat(categories).Distinct().Count();
        if (opts.RequiredUniqueChars > all)
            return Result.Failure($"RequiredUniqueChars ({opts.RequiredUniqueChars}) exceeds available unique characters ({all}).");

        return Result.Success();
    }

    private static void AddRequiredCategoryChars(List<char> chars, List<string> categories)
    {
        foreach (var cat in categories)
            chars.Add(cat[RandomNumberGenerator.GetInt32(cat.Length)]);
    }

    private static void FillRemainingChars(List<char> chars, PasswordOptions opts, List<string> categories)
    {
        while (chars.Count < opts.RequiredLength || chars.Distinct().Count() < opts.RequiredUniqueChars)
        {
            var set = categories[RandomNumberGenerator.GetInt32(categories.Count)];
            chars.Add(set[RandomNumberGenerator.GetInt32(set.Length)]);
        }
    }

    private static void SecureShuffle(List<char> chars)
    {
        for (int i = chars.Count - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }
    }
}
