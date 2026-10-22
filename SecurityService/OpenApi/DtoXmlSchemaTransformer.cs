using System.Reflection;
using System.Xml;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace SecurityService.OpenApi;

/// <summary>Applies DTO summaries, property descriptions, and examples to ASP.NET Core's OpenAPI schemas.</summary>
public sealed class DtoXmlSchemaTransformer : IOpenApiSchemaTransformer
{
    private static readonly Lazy<XmlDocument?> XmlDocumentation = new(LoadXmlDocumentation);

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        Type? dtoType = context.JsonTypeInfo?.Type;
        MemberInfo? property = context.JsonPropertyInfo?.AttributeProvider as MemberInfo;
        string? memberName = property is not null
            ? $"P:{property.DeclaringType?.FullName}.{property.Name}"
            : dtoType is not null ? $"T:{dtoType.FullName}" : null;

        if (memberName is null)
        {
            return Task.CompletedTask;
        }

        XmlNode? member = XmlDocumentation.Value?.SelectSingleNode($"/doc/members/member[@name='{memberName}']");
        if (member is null)
        {
            return Task.CompletedTask;
        }

        string? description = Normalize(member.SelectSingleNode("summary")?.InnerText);
        if (!string.IsNullOrWhiteSpace(description))
        {
            schema.Description = description;
        }

        string? example = Normalize(member.SelectSingleNode("example")?.InnerText);
        if (!string.IsNullOrWhiteSpace(example))
        {
            try
            {
                schema.Example = System.Text.Json.Nodes.JsonNode.Parse(example);
            }
            catch (System.Text.Json.JsonException)
            {
                // Ignore malformed XML example markup rather than failing OpenAPI generation.
            }
        }

        return Task.CompletedTask;
    }

    private static XmlDocument? LoadXmlDocumentation()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "SecurityService.DataTransferObjects.xml");
        if (!File.Exists(path))
        {
            return null;
        }

        XmlDocument document = new();
        document.Load(path);
        return document;
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
