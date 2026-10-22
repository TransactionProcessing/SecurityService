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
        XmlDocument? documentation = XmlDocumentation.Value;
        string? memberName = GetMemberName(context);
        if (documentation is null || memberName is null)
        {
            return Task.CompletedTask;
        }

        XmlNode? member = documentation.SelectSingleNode($"/doc/members/member[@name='{memberName}']");
        if (member is null)
        {
            return Task.CompletedTask;
        }

        ApplyDescription(schema, member);
        ApplyExample(schema, member);
        return Task.CompletedTask;
    }

    private static string? GetMemberName(OpenApiSchemaTransformerContext context)
    {
        MemberInfo? property = context.JsonPropertyInfo?.AttributeProvider as MemberInfo;
        if (property is not null)
        {
            return $"P:{property.DeclaringType?.FullName}.{property.Name}";
        }

        Type? dtoType = context.JsonTypeInfo?.Type;
        return dtoType is null ? null : $"T:{dtoType.FullName}";
    }

    private static void ApplyDescription(OpenApiSchema schema, XmlNode member)
    {
        string? description = Normalize(member.SelectSingleNode("summary")?.InnerText);
        if (!string.IsNullOrWhiteSpace(description))
        {
            schema.Description = description;
        }
    }

    private static void ApplyExample(OpenApiSchema schema, XmlNode member)
    {
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
