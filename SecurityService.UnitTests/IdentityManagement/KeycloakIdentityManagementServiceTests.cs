namespace SecurityService.UnitTests.IdentityManagement;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Moq;
using SecurityService.BusinessLogic;
using SecurityService.BusinessLogic.IdentityManagement;
using SecurityService.BusinessLogic.Requests;
using SecurityService.DataTransferObjects;
using SecurityService.DataTransferObjects.Requests;
using SecurityService.Handlers;
using Shouldly;
using SimpleResults;
using Xunit;

public class KeycloakIdentityManagementServiceTests
{
    [Fact]
    public async Task CreateClient_UsesProviderSettingsWhenBuildingKeycloakRequest()
    {
        RecordingHttpMessageHandler handler = new RecordingHttpMessageHandler(request =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/protocol/openid-connect/token", StringComparison.Ordinal))
            {
                return JsonResponse("{\"access_token\":\"token\",\"expires_in\":300}");
            }

            return new HttpResponseMessage(HttpStatusCode.Created);
        });
        KeycloakIdentityManagementService service = new KeycloakIdentityManagementService(BuildServiceOptions(), new HttpClient(handler));

        Result result = await service.CreateClient(
            new SecurityServiceCommands.CreateClientCommand(
                "test-client",
                "super-secret",
                "Test Client",
                "Description",
                new List<String> { "openid" },
                new List<String> { "client_credentials" },
                "https://portal.example.com",
                new List<String> { "https://portal.example.com/signin" },
                new List<String> { "https://portal.example.com/signout" },
                false,
                false,
                new Dictionary<String, Object>
                {
                    {
                        "keycloak", new Dictionary<String, Object>
                        {
                            { "realm", "transaction-processing" },
                            { "service_accounts_enabled", true },
                            { "web_origins", new List<String> { "https://portal.example.com" } }
                        }
                    }
                }),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        handler.Requests.Count.ShouldBe(2);
        handler.Requests[1].RequestUri.ShouldContain("/admin/realms/transaction-processing/clients");

        using JsonDocument document = JsonDocument.Parse(handler.Requests[1].Body);
        document.RootElement.GetProperty("serviceAccountsEnabled").GetBoolean().ShouldBeTrue();
        document.RootElement.GetProperty("webOrigins")[0].GetString().ShouldBe("https://portal.example.com");
    }

    [Fact]
    public async Task CreateUser_UsesProviderSettingsWhenBuildingKeycloakRequest()
    {
        RecordingHttpMessageHandler handler = new RecordingHttpMessageHandler(request =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/protocol/openid-connect/token", StringComparison.Ordinal))
            {
                return JsonResponse("{\"access_token\":\"token\",\"expires_in\":300}");
            }

            if (request.RequestUri.AbsolutePath.EndsWith("/users", StringComparison.Ordinal) && request.Method == HttpMethod.Post)
            {
                return new HttpResponseMessage(HttpStatusCode.Created);
            }

            return JsonResponse("[{\"id\":\"8a927e15-ac95-421a-b1dc-2ef76ac6096b\",\"username\":\"user@example.com\"}]");
        });
        KeycloakIdentityManagementService service = new KeycloakIdentityManagementService(BuildServiceOptions(), new HttpClient(handler));

        Result result = await service.CreateUser(
            new SecurityServiceCommands.CreateUserCommand(
                Guid.Parse("8a927e15-ac95-421a-b1dc-2ef76ac6096b"),
                "Given",
                "Middle",
                "Family",
                "user@example.com",
                "Password123!",
                "user@example.com",
                "1234567890",
                new Dictionary<String, String> { { "department", "finance" } },
                null,
                new Dictionary<String, Object>
                {
                    {
                        "keycloak", new Dictionary<String, Object>
                        {
                            { "realm", "transaction-processing" },
                            { "enabled", true },
                            { "email_verified", false },
                            { "required_actions", new List<String> { "VERIFY_EMAIL" } }
                        }
                    }
                }),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        handler.Requests[1].RequestUri.ShouldContain("/admin/realms/transaction-processing/users");

        using JsonDocument document = JsonDocument.Parse(handler.Requests[1].Body);
        document.RootElement.GetProperty("enabled").GetBoolean().ShouldBeTrue();
        document.RootElement.GetProperty("emailVerified").GetBoolean().ShouldBeFalse();
        document.RootElement.GetProperty("requiredActions")[0].GetString().ShouldBe("VERIFY_EMAIL");
    }

    [Fact]
    public async Task ClientHandler_CreateClient_PassesProviderSettingsIntoCommand()
    {
        Mock<IMediator> mediator = new Mock<IMediator>();
        SecurityServiceCommands.CreateClientCommand capturedCommand = null;
        mediator.Setup(m => m.Send(It.IsAny<SecurityServiceCommands.CreateClientCommand>(), It.IsAny<CancellationToken>()))
            .Callback<SecurityServiceCommands.CreateClientCommand, CancellationToken>((command, _) => capturedCommand = command)
            .ReturnsAsync(Result.Success());

        await ClientHandler.CreateClient(mediator.Object,
            new CreateClientRequest
            {
                ClientId = "client",
                Secret = "secret",
                ProviderSettings = new Dictionary<String, Object> { { "keycloak", new Dictionary<String, Object> { { "realm", "realm-a" } } } }
            },
            CancellationToken.None);

        capturedCommand.ShouldNotBeNull();
        capturedCommand.ProviderSettings.ShouldNotBeNull();
        capturedCommand.ProviderSettings.ContainsKey("keycloak").ShouldBeTrue();
    }

    [Fact]
    public async Task UserHandler_CreateUser_PassesProviderSettingsIntoCommand()
    {
        Mock<IMediator> mediator = new Mock<IMediator>();
        SecurityServiceCommands.CreateUserCommand capturedCommand = null;
        mediator.Setup(m => m.Send(It.IsAny<SecurityServiceCommands.CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .Callback<SecurityServiceCommands.CreateUserCommand, CancellationToken>((command, _) => capturedCommand = command)
            .ReturnsAsync(Result.Success());

        await UserHandler.CreateUser(mediator.Object,
            new CreateUserRequest
            {
                EmailAddress = "user@example.com",
                ProviderSettings = new Dictionary<String, Object> { { "keycloak", new Dictionary<String, Object> { { "enabled", true } } } }
            },
            CancellationToken.None);

        capturedCommand.ShouldNotBeNull();
        capturedCommand.ProviderSettings.ShouldNotBeNull();
        capturedCommand.ProviderSettings.ContainsKey("keycloak").ShouldBeTrue();
    }

    private static ServiceOptions BuildServiceOptions() =>
        new ServiceOptions
        {
            IdentityProvider = "Keycloak",
            Keycloak = new KeycloakOptions
                       {
                           AdminClientId = "admin-cli",
                           AdminClientSecret = "secret",
                           AdminRealm = "master",
                           Realm = "default-realm",
                           ServerUrl = "https://keycloak.example.com"
                       }
        };

    private static HttpResponseMessage JsonResponse(String body) =>
        new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

    private sealed class RecordingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> Responder;

        public RecordingHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            this.Responder = responder;
        }

        public List<RecordedRequest> Requests { get; } = new List<RecordedRequest>();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.Requests.Add(new RecordedRequest
                              {
                                  Body = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken),
                                  Method = request.Method.Method,
                                  RequestUri = request.RequestUri!.ToString()
                              });
            return this.Responder(request);
        }
    }

    private sealed class RecordedRequest
    {
        public String Body { get; set; }

        public String Method { get; set; }

        public String RequestUri { get; set; }
    }
}
