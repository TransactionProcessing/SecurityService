using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecurityServiceTestUI
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Net.Http;
    using HealthChecks.UI.Client;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.IdentityModel.Tokens;

    public class Startup
    {
        public Startup(IWebHostEnvironment webHostEnvironment)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(webHostEnvironment.ContentRootPath)
                                                                                   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                                                                   .AddJsonFile($"appsettings.{webHostEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                                                                                   .AddEnvironmentVariables();
            Startup.Configuration = builder.Build();
        }

        public static IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            String authority = OpenIdConnectConfiguration.GetAuthority(Configuration);
            String issuerUrl = OpenIdConnectConfiguration.GetIssuerUrl(Configuration);
            String metadataAddress = OpenIdConnectConfiguration.GetMetadataAddress(Configuration);
            String authorizationEndpointOverride = OpenIdConnectConfiguration.GetAuthorizationEndpointOverride(Configuration);

            Console.WriteLine($"Authority is {authority}");
            Console.WriteLine($"Issuer Url is {issuerUrl}");
            
            services.AddHealthChecks();

            services.AddAuthentication(options =>
                                       {
                                           options.DefaultScheme = "Cookies";
                                           options.DefaultChallengeScheme = "oidc";
                                       })
                    .AddCookie("Cookies")
                    .AddOpenIdConnect("oidc", options =>
                                              {
                                                  HttpClientHandler handler = new HttpClientHandler();
                                                  handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                                                  options.BackchannelHttpHandler = handler;

                                                  options.Authority = authority;
                                                  options.TokenValidationParameters = new TokenValidationParameters
                                                                                      {
                                                                                          ValidateAudience = false,
                                                                                          ValidIssuer = issuerUrl
                                                                                      };
                                                  options.RequireHttpsMetadata = Configuration.GetValue<Boolean>("AppSettings:RequireHttpsMetadata", true);
                                                   
                                                  options.ClientSecret =
                                                      Configuration.GetValue<String>("AppSettings:ClientSecret");
                                                  options.ClientId = Configuration.GetValue<String>("AppSettings:ClientId");

                                                  options.MetadataAddress = metadataAddress;

                                                  options.ResponseType = "code id_token";

                                                  options.Scope.Clear();
                                                  options.Scope.Add("openid");
                                                  options.Scope.Add("profile");
                                                  options.Scope.Add("email");
                                                  options.Scope.Add("offline_access");

                                                  options.ClaimActions.MapAllExcept("iss",
                                                                                    "nbf",
                                                                                    "exp",
                                                                                    "aud",
                                                                                    "nonce",
                                                                                    "iat",
                                                                                    "c_hash");

                                                  options.GetClaimsFromUserInfoEndpoint = true;
                                                  options.SaveTokens = true;
                                                  
                                                  options.Events.OnRedirectToIdentityProvider = context =>
                                                  {
                                                      if (String.IsNullOrWhiteSpace(authorizationEndpointOverride) == false)
                                                      {
                                                          // Intercept the redirection so the browser navigates to the right URL in your host
                                                          context.ProtocolMessage.IssuerAddress = authorizationEndpointOverride;
                                                      }

                                                      return Task.CompletedTask;
                                                  };
                                              });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
                app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");
            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    app.UseHsts();
            //}
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
                             {
                                 endpoints.MapDefaultControllerRoute();
                                 endpoints.MapHealthChecks("health",
                                                           new HealthCheckOptions
                                                           {
                                                               Predicate = _ => true,
                                                               ResponseWriter = Shared.HealthChecks.HealthCheckMiddleware.WriteResponse
                                                           });
                                 endpoints.MapHealthChecks("healthui",
                                                           new HealthCheckOptions
                                                           {
                                                               Predicate = _ => true,
                                                               ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                                                           });
                             });
        }
    }
}
