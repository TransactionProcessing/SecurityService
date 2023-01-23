﻿using Lamar;
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
using System.Threading.Tasks;
using Xunit;

namespace SecurityService.UnitTests
{
    using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
    using System.Diagnostics;
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
            this.Requests.Add(TestData.CreateApiResourceRequest);
            this.Requests.Add(TestData.GetApiResourceRequest);
            this.Requests.Add(TestData.GetApiResourcesRequest);
            this.Requests.Add(TestData.CreateApiScopeRequest);
            this.Requests.Add(TestData.GetApiScopeRequest);
            this.Requests.Add(TestData.GetApiScopesRequest);
            this.Requests.Add(TestData.CreateClientRequest);
            this.Requests.Add(TestData.GetClientRequest);
            this.Requests.Add(TestData.GetClientsRequest);
            this.Requests.Add(TestData.CreateIdentityResourceRequest);
            this.Requests.Add(TestData.GetIdentityResourceRequest);
            this.Requests.Add(TestData.GetIdentityResourcesRequest);
            this.Requests.Add(TestData.CreateUserRequest);
            this.Requests.Add(TestData.GetUserRequest);
            this.Requests.Add(TestData.GetUsersRequest);
            this.Requests.Add(TestData.CreateRoleRequest);
            this.Requests.Add(TestData.GetRoleRequest);
            this.Requests.Add(TestData.GetRolesRequest);
            this.Requests.Add(TestData.ChangeUserPasswordRequest);
            this.Requests.Add(TestData.ConfirmUserEmailAddressRequest);
            this.Requests.Add(TestData.ProcessPasswordResetConfirmationRequest);
            this.Requests.Add(TestData.ProcessPasswordResetRequest);
            this.Requests.Add(TestData.SendWelcomeEmailRequest);
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

            RoleManager<IdentityRole> authDb = Startup.Container.GetInstance<RoleManager<IdentityRole>>();
           
            List<String> errors = new List<String>();
            IMediator mediator = Startup.Container.GetService<IMediator>();
            foreach (IBaseRequest baseRequest in this.Requests)
            {
                try
                {
                    if (baseRequest is CreateUserRequest ||
                        baseRequest is GetRoleRequest){
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
                                          });
        }
    }
}
