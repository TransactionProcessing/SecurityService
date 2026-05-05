using System.Text.Json;
using System.Text.Json.Serialization;
using SecurityService.Configuration;
using Shouldly;

namespace SecurityService.UnitTests.Configuration;

public class JsonSerializerConfigurationTests
{
    [Fact]
    public void ConfigureMinimalApi_AppliesLegacyCompatibleDefaults()
    {
        JsonSerializerOptions options = new();

        JsonSerializerConfiguration.ConfigureMinimalApi(options);

        options.PropertyNamingPolicy.ShouldBe(JsonNamingPolicy.SnakeCaseLower);
        options.WriteIndented.ShouldBeTrue();

        var payload = new SerializerPayload
        {
            CreatedDate = new DateTime(2026, 03, 23, 17, 08, 40, DateTimeKind.Local)
        };

        var json = System.Text.Json.JsonSerializer.Serialize(payload, options);

        json.ShouldContain("created_date");
        json.ShouldContain("Z");
    }

    private sealed class SerializerPayload
    {
        public DateTime CreatedDate { get; init; }
    }
}
