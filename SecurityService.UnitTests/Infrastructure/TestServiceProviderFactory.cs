using System.Security.Cryptography;
using MessagingService.Client;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SecurityService.BusinessLogic;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Database.DbContexts;
using SecurityService.Database.Entities;

namespace SecurityService.UnitTests.Infrastructure;

public static class TestServiceProviderFactory
{
    public static ServiceProvider Create(string databaseName, UserManager<ApplicationUser>? userManager = null, SignInManager<ApplicationUser>? signInManager = null)
    {
        var rsa = RSA.Create(2048);
        var key = new RsaSecurityKey(rsa);
        
        var services = new ServiceCollection();
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
        services.AddSingleton(signingCredentials);
        services.Configure<ServiceOptions>(options => {
            options.ClientId = "client";
            options.ClientSecret = "SECRET";
            options.IssuerUrl = "https://localhost:5001";
            options.InMemoryDatabaseName = databaseName;
            options.SeedDefaultScopes = true;
        });
        services.AddSingleton<IClientJwtService, ClientJwtService>();
        services.AddLogging();
        services.AddDbContext<SecurityServiceDbContext>(options =>
        {
            options.UseInMemoryDatabase(databaseName);
            options.UseOpenIddict();
        });
        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<SecurityServiceDbContext>()
            .AddDefaultTokenProviders();
        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore().UseDbContext<SecurityServiceDbContext>();
            }).AddServer(serverOptions => {
                serverOptions.AddSigningKey(key);
            });
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(typeof(SecurityServiceCommands).Assembly));
        services.AddSingleton<IMessagingServiceClient, TestMessagingServiceClient>();
        if (userManager != null)
            services.AddSingleton(userManager);
        
        if (signInManager != null)
            services.AddSingleton(signInManager);
        

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();
        dbContext.Database.EnsureCreated();
        return provider;
    }
}
