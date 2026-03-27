using System.Text.Json;

namespace SecurityService.Database;

public static class JsonListSerializer
{
    public static string Serialize(IEnumerable<string> values) => JsonSerializer.Serialize(values.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value));

    public static IReadOnlyCollection<string> Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<string>();
        }

        return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
    }
}
