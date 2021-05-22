using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

using BlueBoxMoon.LocalSubway.Messages;
using BlueBoxMoon.LocalSubway.Server.Authentication;
using BlueBoxMoon.LocalSubway.Sessions;
using BlueBoxMoon.LocalSubway.Tunnels;

using Microsoft.AspNetCore.Http;

namespace BlueBoxMoon.LocalSubway.Server
{
    /// <summary>
    /// Handles a single session for a client that is connected to us over
    /// a web socket connection.
    /// </summary>
    /// <seealso cref="BlueBoxMoon.LocalSubway.Sessions.ServerSession" />
    public class SubwayServerSession : ServerSession
    {
        #region Fields

        /// <summary>
        /// The domain manager
        /// </summary>
        private readonly SubwayDomainManager _domainManager;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SubwayServerSession"/> class.
        /// </summary>
        /// <param name="socket">The socket the session will use.</param>
        /// <param name="domainManager">The domain manager.</param>
        public SubwayServerSession( WebSocket socket, SubwayDomainManager domainManager )
            : base( socket, new JsonConverter() )
        {
            _domainManager = domainManager;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Removes the tunnel from the server and closes all connections.
        /// </summary>
        /// <param name="tunnel">The tunnel to be removed.</param>
        /// <param name="notifyClient">if set to <c>true</c> then notify the client of the closure.</param>
        protected override Task RemoveTunnelAsync( Tunnel tunnel, bool notifyClient )
        {
            if ( tunnel is WebTunnel webTunnel )
            {
                _domainManager.RemoveTunnel( webTunnel );
            }

            return base.RemoveTunnelAsync( tunnel, notifyClient );
        }

        /// <summary>
        /// Creates a new TCP tunnel to the client.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="port">The port to listen on.</param>
        /// <returns>
        /// A <see cref="T:BlueBoxMoon.LocalSubway.Messages.Response" /> to be sent back to the client.
        /// </returns>
        protected override Task<Response> CreateTcpTunnelAsync( Guid messageId, int port )
        {
            var tunnel = new TcpListenerTunnel( this, port );

            AddTunnel( tunnel );

            var response = new Response( messageId, true, $"Tunnel created on port {tunnel.Port}." );
            response.Values["tunnel_id"] = tunnel.Id;
            response.Values["port"] = tunnel.Port;

            return Task.FromResult( response );
        }

        /// <summary>
        /// Creates a new web tunnel to the client.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="domain">The domain to be used for the tunnel.</param>
        /// <returns>
        /// A <see cref="T:BlueBoxMoon.LocalSubway.Messages.Response" /> to be sent back to the client.
        /// </returns>
        protected override Task<Response> CreateWebTunnelAsync( Guid messageId, string domain )
        {
            var tunnel = new WebTunnel( this, domain );

            if ( _domainManager.AddTunnel( tunnel ) )
            {
                AddTunnel( tunnel );

                var url = $"https://{domain}.{_domainManager.Domain}";
                var response = new Response( messageId, true, $"Tunnel created at {url}" );
                response.Values["tunnel_id"] = tunnel.Id;
                response.Values["url"] = url;

                return Task.FromResult( response );
            }
            else
            {
                return Task.FromResult( new Response( messageId, false, $"Tunnel domain already in use." ) );
            }
        }

        #endregion
    }
}
