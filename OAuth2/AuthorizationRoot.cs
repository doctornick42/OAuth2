using System;
using System.Collections.Generic;
using System.Reflection;
using OAuth2.Client;
using OAuth2.Configuration;
using OAuth2.Infrastructure;
using System.Linq;

namespace OAuth2
{
    public class AuthorizationRoot
    {
        protected readonly IRequestFactory _requestFactory;
        protected readonly OAuth2ConfigurationSection _configurationSection;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationRoot" /> class.
        /// </summary>
        /// <remarks>
        /// Since this is boundary class, we decided to create 
        /// parameterless constructor where default implementations of dependencies are used.
        /// So, despite we encourage you to employ IoC pattern, 
        /// you are still able to just create instance of manager manually and then use it in your project.
        /// </remarks>
        public AuthorizationRoot() : 
            this(new ConfigurationManager(), "oauth2", new RequestFactory())
        {
            _clientsTypes = new List<Type>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationRoot" /> class.
        /// </summary>
        /// <param name="configurationManager">The configuration manager.</param>
        /// <param name="configurationSectionName">Name of the configuration section.</param>
        /// <param name="requestFactory">The request factory.</param>
        public AuthorizationRoot(
            IConfigurationManager configurationManager, 
            string configurationSectionName, 
            IRequestFactory requestFactory)
        {
            _requestFactory = requestFactory;
            _configurationSection = configurationManager
                .GetConfigSection<OAuth2ConfigurationSection>(configurationSectionName);
        }
        
        /// <summary>
        /// Returns collection of clients which were configured 
        /// using application configuration file and are enabled.
        /// </summary>
        public virtual IEnumerable<IClient> Clients
        {
            get
            {
                Func<ClientConfiguration, Type> getType = 
                    configuration => _clientsTypes.FirstOrDefault(x => x.Name == configuration.ClientTypeName);

                return
                    _configurationSection.Services.AsEnumerable()
                                        .Where(configuration => configuration.IsEnabled)
                                        .Select(configuration => new { configuration, type = getType(configuration) })
                                        .Where(o => o.type != null)
                                        .Select(o => (IClient)Activator.CreateInstance(o.type, _requestFactory, o.configuration));                
            }
        }

        /// <summary>
        /// List of available clients' types
        /// </summary>
        protected List<Type> _clientsTypes;

        /// <summary>
        /// Register client type
        /// </summary>
        public void RegisterClient<TClient>()
            where TClient : IClient
        {
            _clientsTypes.Add(typeof(TClient));
        }

        /// <summary>
        /// Unregister client type
        /// </summary>
        public void UnregisterClient<TClient>()
            where TClient : IClient
        {
            //if (_clientsTypes.Contains(typeof(TClient)))
            //{
                _clientsTypes.Remove(typeof(TClient));
            //}
        }
    }
}