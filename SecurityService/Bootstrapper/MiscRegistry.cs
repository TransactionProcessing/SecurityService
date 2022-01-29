namespace SecurityService.Bootstrapper
{
    using BusinessLogic;
    using Factories;
    using Lamar;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Lamar.ServiceRegistry" />
    public class MiscRegistry : ServiceRegistry
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MiscRegistry"/> class.
        /// </summary>
        public MiscRegistry()
        {
            this.AddScoped<ISecurityServiceManager, SecurityServiceManager>();
            this.AddSingleton<IModelFactory, ModelFactory>();
        }

        #endregion
    }
}