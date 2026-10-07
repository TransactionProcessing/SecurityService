using SimpleResults;

namespace SecurityService.BusinessLogic.Validation;

public static class ClientRedirectUriValidator
{
    public static Result<List<Uri>> Validate(
        IEnumerable<string> values,
        string propertyName)
    {
        List<Uri> uris = new();

        foreach (string value in values
                     .Where(value => string.IsNullOrWhiteSpace(value) == false)
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) == false || IsAllowed(uri, value) == false)
            {
                return Result.Invalid($"{propertyName} contains an invalid or unsafe URI.");
            }

            uris.Add(uri);
        }

        return Result.Success(uris);
    }

    private static bool IsAllowed(Uri uri, string value)
    {
        if (value.Contains('*', StringComparison.Ordinal) ||
            string.IsNullOrEmpty(uri.UserInfo) == false ||
            string.IsNullOrEmpty(uri.Fragment) == false)
        {
            return false;
        }

        if (uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) && uri.IsLoopback;
    }
}
