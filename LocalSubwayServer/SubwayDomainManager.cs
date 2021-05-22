using System.Collections.Concurrent;

using BlueBoxMoon.LocalSubway.Server.Configuration;

using Microsoft.Extensions.Configuration;

namespace BlueBoxMoon.LocalSubway.Server
{
    /// <summary>
    /// Handles requests to all registered sub domains. In the future this
    /// should be configurable for which domains to be tracked.
    /// </summary>
    public class SubwayDomainManager
    {
        #region Fields

        /// <summary>
        /// The tunnels that are currently registered.
        /// </summary>
        private readonly ConcurrentDictionary<string, WebTunnel> _tunnels = new ConcurrentDictionary<string, WebTunnel>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the domain that is being handled by this instance.
        /// </summary>
        /// <value>
        /// The domain that is being handled by this instance.
        /// </value>
        public string Domain { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SubwayDomainManager"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public SubwayDomainManager( IConfiguration configuration )
        {
            var config = configuration.Get<BasicConfiguration>();

            Domain = config.Domain ?? "localhost";
        }

        #endregion

        #region Methods

        /// <summary>
        /// Finds the tunnel for the specified subdomain name.
        /// </summary>
        /// <param name="subdomain">The subdomain name.</param>
        /// <returns>The matching <see cref="WebTunnel"/> or <c>null</c> if no tunnel was found.</returns>
        public WebTunnel FindTunnel( string subdomain )
        {
            return _tunnels.TryGetValue( subdomain, out var tunnel ) ? tunnel : null;
        }

        /// <summary>
        /// Attempts to add the tunnel.
        /// </summary>
        /// <param name="tunnel">The tunnel to be added.</param>
        /// <returns><c>true</c> if the tunnel was added or <c>false</c> if there was a subdomain name conflict.</returns>
        public bool AddTunnel( WebTunnel tunnel )
        {
            return _tunnels.TryAdd( tunnel.Subdomain, tunnel );
        }

        /// <summary>
        /// Removes the tunnel.
        /// </summary>
        /// <param name="tunnel">The tunnel to be removed.</param>
        public void RemoveTunnel( WebTunnel tunnel )
        {
            _tunnels.TryRemove( tunnel.Subdomain, out var _ );
        }

        #endregion
    }
}
