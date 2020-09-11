using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SecurityServiceTestWebClient
{
    using System.IdentityModel.Tokens.Jwt;
    using IdentityModel;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Http;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;
    using TestClientUserInterface.TokenManagement;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.Configure<CookiePolicyOptions>(options =>
                                                    {
                                                        // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                                                        options.CheckConsentNeeded = context => true;
                                                        options.MinimumSameSitePolicy = SameSiteMode.None;
                                                    });

            services.AddAuthentication(options =>
                                       {
                                           options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                                           options.DefaultChallengeScheme = "oidc";
                                       }).AddCookie(options =>
                                                    {
                                                        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                                                        options.Cookie.Name = "mvchybridautorefresh";
                                                    }).AddAutomaticTokenManagement().AddOpenIdConnect("oidc",
                                                                                                      options =>
                                                                                                      {
                                                                                                          options.SignInScheme = "Cookies";
                                                                                                          options.Authority = Configuration.GetValue<String>("Authority");
                                                                                                          
                                                                                                          options.RequireHttpsMetadata = false;

                                                                                                          options.ClientSecret =
                                                                                                              Configuration.GetValue<String>("ClientSecret");
                                                                                                          options.ClientId = Configuration.GetValue<String>("ClientId");

                                                                                                          options.MetadataAddress = $"{Configuration.GetValue<String>("Authority")}/.well-known/openid-configuration";

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
                                                                                                                                                            // Intercept the redirection so the browser navigates to the right URL in your host
                                                                                                                                                            context.ProtocolMessage.IssuerAddress = $"{Configuration.GetValue<String>("Authority")}/connect/authorize";
                                                                                                                                                            return Task.CompletedTask;
                                                                                                                                                        };
                                                                                                      });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
