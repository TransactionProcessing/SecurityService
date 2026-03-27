using Microsoft.Data.SqlClient;
using SecurityService.BusinessLogic;

namespace SecurityService.Database.DbContexts
{
    using Duende.IdentityServer.EntityFramework.DbContexts;
    using Duende.IdentityServer.EntityFramework.Options;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Shared.General;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class AuthenticationDbContext : IdentityDbContext<ApplicationUser> {
        public AuthenticationDbContext(DbContextOptions options) : base(options) {
        }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
    public static class Extensions
    {

        public static async Task SetDbInSimpleMode(this DbContext context, CancellationToken cancellationToken)
        {
            var dbName = context.Database.GetDbConnection().Database;

            var connection = context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);
            
            // 1. Check current recovery model
            await using var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = @"
SELECT recovery_model_desc
FROM sys.databases
WHERE name = @dbName;
";
            var param = checkCommand.CreateParameter();
            param.ParameterName = "@dbName";
            param.Value = dbName;
            checkCommand.Parameters.Add(param);

            var result = await checkCommand.ExecuteScalarAsync(cancellationToken);
            var currentRecoveryModel = result?.ToString();

            if (currentRecoveryModel != "SIMPLE")
            {
                // 2. Alter database outside transaction
                await using var alterCommand = connection.CreateCommand();
                alterCommand.CommandText = $@"
ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
ALTER DATABASE [{dbName}] SET RECOVERY SIMPLE;
ALTER DATABASE [{dbName}] SET MULTI_USER;
";
                // Execute outside EF transaction
                await alterCommand.ExecuteNonQueryAsync(cancellationToken);
            }
        }
    }


}