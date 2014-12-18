using System.Configuration;
using System.Configuration.Provider;
using System.Web.Configuration;

namespace PersistenceService
{
    public class Persistence
    {
        private static readonly Persistence _instance = new Persistence();
        private static bool _isInitialized;
        private static ProviderCollection _providers;
        private static PersistenceProvider _provider;

        static Persistence()
        {
            _instance.Initialize();
        }

        public static Persistence Instance
        {
            get { return _instance; }
        }

        /// <summary>
        ///     Returns a list of all configured persistence providers.
        /// </summary>
        public ProviderCollection Providers
        {
            get { return _providers; }
        }

        /// <summary>
        ///     Returns the active Persistence provider configured.
        /// </summary>
        public PersistenceProvider Provider
        {
            get { return _provider; }
        }

        private void Initialize()
        {
            if (!_isInitialized)
            {
                PersistenceProviderConfigurationSection Config;
                Config = (PersistenceProviderConfigurationSection)ConfigurationManager.GetSection("persistence");
                if (Config == null)
                    throw new ConfigurationErrorsException(
                        "Persistence is not configured to be used with this application.");
                _providers = new ProviderCollection();

                ProvidersHelper.InstantiateProviders(Config.Providers, _providers, typeof(PersistenceProvider));
                _provider = _providers[Config.DefaultProvider] as PersistenceProvider;
                _isInitialized = true;
            }
        }
    }
}
