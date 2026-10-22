using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace SecurityService.OpenApi;

/// <summary>Describes an OpenAPI media type example attached to an endpoint.</summary>
public sealed record EndpointExampleMetadata(
    EndpointExampleLocation Location,
    string Json,
    string ContentType = "application/json",
    string StatusCode = "200",
    string Summary = "Example");

public enum EndpointExampleLocation
{
    Request,
    Response
}

public static class EndpointExampleExtensions
{
    public static RouteHandlerBuilder WithRequestExample(this RouteHandlerBuilder builder, string json, string summary = "Example request", string contentType = "application/json")
    {
        return builder.WithMetadata(new EndpointExampleMetadata(EndpointExampleLocation.Request, json, contentType, Summary: summary));
    }

    public static RouteHandlerBuilder WithResponseExample(this RouteHandlerBuilder builder, string json, string summary = "Example response", string statusCode = "200", string contentType = "application/json")
    {
        return builder.WithMetadata(new EndpointExampleMetadata(EndpointExampleLocation.Response, json, contentType, statusCode, summary));
    }
}

/// <summary>Adds the same request and response examples to ASP.NET Core's OpenAPI document used by Scalar.</summary>
public sealed class EndpointExampleOpenApiTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        IEnumerable<EndpointExampleMetadata> examples = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<EndpointExampleMetadata>();

        foreach (EndpointExampleMetadata example in examples)
        {
            OpenApiMediaType? mediaType = example.Location switch
            {
                EndpointExampleLocation.Request when operation.RequestBody?.Content.TryGetValue(example.ContentType, out OpenApiMediaType? requestContent) == true => requestContent,
                EndpointExampleLocation.Response when operation.Responses.TryGetValue(example.StatusCode, out IOpenApiResponse? response) => GetOrAddResponseContent(response, example.ContentType),
                _ => null
            };

            if (mediaType is null)
            {
                continue;
            }

            JsonNode? json = JsonNode.Parse(example.Json);
            mediaType.Examples ??= new Dictionary<string, IOpenApiExample>();
            mediaType.Examples["Example"] = new OpenApiExample
            {
                Summary = example.Summary,
                Value = json?.DeepClone()
            };
        }

        return Task.CompletedTask;
    }

    private static OpenApiMediaType GetOrAddResponseContent(IOpenApiResponse response, string contentType)
    {
        if (response.Content.TryGetValue(contentType, out OpenApiMediaType? content))
        {
            return content;
        }

        content = new OpenApiMediaType();
        response.Content[contentType] = content;
        return content;
    }
}
