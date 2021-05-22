using BlueBoxMoon.LocalSubway.Sessions;
using BlueBoxMoon.LocalSubway.Tunnels;

namespace BlueBoxMoon.LocalSubway.Server
{
    /// <summary>
    /// A tunnel for web traffic that has been configured for the client.
    /// </summary>
    /// <seealso cref="BlueBoxMoon.LocalSubway.Tunnels.Tunnel" />
    public class WebTunnel : Tunnel
    {
        #region Properties

        /// <summary>
        /// Gets the subdomain that this tunnel is using.
        /// </summary>
        /// <value>
        /// The subdomain that this tunnel is using.
        /// </value>
        public string Subdomain { get; }

        /// <summary>
        /// Gets the session owning this tunnel.
        /// </summary>
        /// <value>
        /// The session owning this tunnel.
        /// </value>
        public ServerSession Session { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WebTunnel"/> class.
        /// </summary>
        /// <param name="session">The owning session.</param>
        /// <param name="subdomain">The subdomain in use.</param>
        public WebTunnel( ServerSession session, string subdomain )
        {
            Session = session;
            Subdomain = subdomain;
        }

        #endregion
    }
}
