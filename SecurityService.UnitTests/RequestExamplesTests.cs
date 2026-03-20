namespace SecurityService.UnitTests
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using SecurityService.Common.Examples;
    using SecurityService.DataTransferObjects;
    using SecurityService.DataTransferObjects.Requests;
    using Shouldly;
    using Xunit;

    public class RequestExamplesTests
    {
        [Fact]
        public void CreateClientRequest_SerializesProviderSettingsUsingProviderSettingsPropertyName()
        {
            CreateClientRequest request = new CreateClientRequest
                                          {
                                              ProviderSettings = new Dictionary<string, object>
                                                                 {
                                                                     {
                                                                         "keycloak", new Dictionary<string, object>
                                                                                      {
                                                                                          { "realm", "transaction-processing" }
                                                                                      }
                                                                     }
                                                                 }
                                          };

            JObject serialized = JObject.Parse(JsonConvert.SerializeObject(request));

            serialized["provider_settings"].ShouldNotBeNull();
            serialized["provider_settings"]["keycloak"]["realm"]!.Value<string>().ShouldBe("transaction-processing");
        }

        [Fact]
        public void CreateClientRequestExample_IncludesProviderSettings()
        {
            CreateClientRequest example = new CreateClientRequestExample().GetExamples();

            example.ProviderSettings.ShouldNotBeNull();
            example.ProviderSettings.ContainsKey("keycloak").ShouldBeTrue();
        }

        [Fact]
        public void CreateUserRequest_SerializesProviderSettingsUsingProviderSettingsPropertyName()
        {
            CreateUserRequest request = new CreateUserRequest
                                        {
                                            ProviderSettings = new Dictionary<string, object>
                                                               {
                                                                   {
                                                                       "keycloak", new Dictionary<string, object>
                                                                                    {
                                                                                        { "enabled", true }
                                                                                    }
                                                                   }
                                                               }
                                        };

            JObject serialized = JObject.Parse(JsonConvert.SerializeObject(request));

            serialized["provider_settings"].ShouldNotBeNull();
            serialized["provider_settings"]["keycloak"]["enabled"]!.Value<bool>().ShouldBeTrue();
        }

        [Fact]
        public void CreateUserRequestExample_IncludesProviderSettings()
        {
            CreateUserRequest example = new CreateUserRequestExample().GetExamples();

            example.ProviderSettings.ShouldNotBeNull();
            example.ProviderSettings.ContainsKey("keycloak").ShouldBeTrue();
        }
    }
}
