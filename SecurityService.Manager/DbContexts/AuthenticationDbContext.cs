namespace SecurityService.Manager.DbContexts
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext{Microsoft.AspNetCore.Identity.IdentityUser}" />
    public class AuthenticationDbContext : IdentityDbContext<IdentityUser>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationDbContext"/> class.
        /// </summary>
        public AuthenticationDbContext()
        {
            // Paramaterless constructor required for migrations.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationDbContext"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : base(options)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when [configuring].
        /// </summary>
        /// <param name="builder">The builder.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            base.OnConfiguring(builder);
        }

        /// <summary>
        /// Configures the schema needed for the identity framework.
        /// </summary>
        /// <param name="builder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        #endregion
    }
}