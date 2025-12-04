using Lamar;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MessagingService.Client;
using MessagingService.DataTransferObjects;
using SimpleResults;
using Xunit;

namespace SecurityService.UnitTests
{
    using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
    using System.Diagnostics;
    using System.IO;
    using BusinessLogic.Requests;
    using Duende.IdentityServer.EntityFramework.DbContexts;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Identity.Client;
    using Swashbuckle.AspNetCore.Filters;
    using SecurityService.BusinessLogic;
    using SecurityService.Database.DbContexts;
    using Duende.IdentityServer;

    public class MediatorTests
    {
        private List<IBaseRequest> Requests = new List<IBaseRequest>();

        public MediatorTests(){
            this.Requests.Add(TestData.CreateApiResourceCommand);
            this.Requests.Add(TestData.GetApiResourceQuery);
            this.Requests.Add(TestData.GetApiResourcesQuery);
            this.Requests.Add(TestData.CreateApiScopeCommand);
            this.Requests.Add(TestData.GetApiScopeQuery);
            this.Requests.Add(TestData.GetApiScopesQuery);
            this.Requests.Add(TestData.CreateClientCommand);
            this.Requests.Add(TestData.GetClientQuery);
            this.Requests.Add(TestData.GetClientsQuery);
            this.Requests.Add(TestData.CreateIdentityResourceCommand);
            this.Requests.Add(TestData.GetIdentityResourceQuery);
            this.Requests.Add(TestData.GetIdentityResourcesQuery);
            this.Requests.Add(TestData.CreateUserCommand);
            this.Requests.Add(TestData.GetUserQuery);
            this.Requests.Add(TestData.GetUsersQuery);
            this.Requests.Add(TestData.CreateRoleCommand);
            this.Requests.Add(TestData.GetRoleQuery);
            this.Requests.Add(TestData.GetRolesQuery);
            this.Requests.Add(TestData.ChangeUserPasswordCommand);
            this.Requests.Add(TestData.ConfirmUserEmailAddressCommand);
            this.Requests.Add(TestData.ProcessPasswordResetConfirmationCommand);
            this.Requests.Add(TestData.ProcessPasswordResetRequestCommand);
            this.Requests.Add(TestData.SendWelcomeEmailCommand);

            Directory.CreateDirectory("D:\\home\\");
        }

        [Fact]
        public async Task Mediator_Send_RequestHandled()
        {
            Shared.Logger.Logger.Initialise(NullLogger.Instance);
            Mock<IWebHostEnvironment> hostingEnvironment = new Mock<IWebHostEnvironment>();
            hostingEnvironment.Setup(he => he.EnvironmentName).Returns("Development");
            hostingEnvironment.Setup(he => he.ContentRootPath).Returns("/home");
            hostingEnvironment.Setup(he => he.ApplicationName).Returns("Test Application");

            ServiceRegistry services = new ServiceRegistry();
            Startup s = new Startup(hostingEnvironment.Object);
            Startup.Configuration = this.SetupMemoryConfiguration();

            this.AddTestRegistrations(services, hostingEnvironment.Object);
            s.ConfigureContainer(services);
            Container container = Startup.GetContainer();
            RoleManager<IdentityRole> authDb = container.GetInstance<RoleManager<IdentityRole>>();
           
            List<String> errors = new List<String>();
            IMediator mediator = container.GetService<IMediator>();
            foreach (IBaseRequest baseRequest in this.Requests)
            {
                try
                {
                    if (baseRequest is SecurityServiceCommands.CreateUserCommand ||
                        baseRequest is SecurityServiceQueries.GetRoleQuery){
                        await authDb.CreateAsync(new IdentityRole(TestData.RoleName));
                    }
                    else{
                        if (await authDb.RoleExistsAsync(TestData.RoleName)){
                            IdentityRole roleToRemove = await authDb.Roles.SingleOrDefaultAsync();
                            await authDb.DeleteAsync(roleToRemove);
                        }
                    }

                    await mediator.Send(baseRequest);
                }
                catch (Exception ex)
                {
                    errors.Add($"[{baseRequest.GetType()}] - {ex.Message}");
                }
            }

            if (errors.Any() == true)
            {
                String errorMessage = String.Join(Environment.NewLine, errors);
                throw new Exception(errorMessage);
            }
        }

        private IConfigurationRoot SetupMemoryConfiguration(){

            Dictionary<String, String> configuration = new Dictionary<String, String>();

            IConfigurationBuilder builder = new ConfigurationBuilder();

            configuration.Add("AppSettings:DatabaseEngine", "SqlServer");
            configuration.Add("ConnectionStrings:PersistedGrantDbContext", "PersistedGrantDbContext");
            configuration.Add("ConnectionStrings:ConfigurationDbContext", "ConfigurationDbContext");
            configuration.Add("ConnectionStrings:AuthenticationDbContext", "AuthenticationDbContext");
            configuration.Add("AppSettings:MessagingServiceApi", "http://localhost");
            configuration.Add("ServiceOptions:PublicOrigin", "https://127.0.0.1:5001");
            configuration.Add("ServiceOptions:ClientId", "ClientId");
            configuration.Add("ServiceOptions:IssuerUrl", "https://127.0.0.1:5001");
            configuration.Add("ServiceOptions:UseInMemoryDatabase", "true");

            builder.AddInMemoryCollection(configuration);

            return builder.Build();
        }

        private void AddTestRegistrations(ServiceRegistry services,
                                          IWebHostEnvironment hostingEnvironment){

            services.AddLogging();
            DiagnosticListener diagnosticSource = new DiagnosticListener(hostingEnvironment.ApplicationName);
            services.AddSingleton<DiagnosticSource>(diagnosticSource);
            services.AddSingleton<DiagnosticListener>(diagnosticSource);
            services.AddSingleton<IConfiguration>(Startup.Configuration);
            services.AddSingleton<IHostEnvironment>(hostingEnvironment);
            services.AddSingleton<IWebHostEnvironment>(hostingEnvironment);
            
            services.OverrideServices(s => {
                                          s.AddDbContext<ConfigurationDbContext>(builder => builder.UseInMemoryDatabase("Configuration"), ServiceLifetime.Singleton);
                                          s.AddDbContext<PersistedGrantDbContext>(builder => builder.UseInMemoryDatabase("PersistedGrantDb"), ServiceLifetime.Singleton);
                                          s.AddDbContext<AuthenticationDbContext>(builder => builder.UseInMemoryDatabase("Authentication"), ServiceLifetime.Singleton);
                                          s.AddSingleton<IMessagingServiceClient, DummyMessagingServiceClient>();
            });
        }
    }

    public class DummyMessagingServiceClient : IMessagingServiceClient {
        public async Task<Result> SendEmail(String accessToken,
                                            SendEmailRequest request,
                                            CancellationToken cancellationToken) => Result.Success();

        public async Task<Result> ResendEmail(String accessToken,
                                              ResendEmailRequest request,
                                              CancellationToken cancellationToken) => Result.Success();

        public async Task<Result> SendSMS(String accessToken,
                                          SendSMSRequest request,
                                          CancellationToken cancellationToken) => Result.Success();
    }
}
