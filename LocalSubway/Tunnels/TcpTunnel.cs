using System;
using System.Threading.Tasks;

using BlueBoxMoon.LocalSubway.Connections;
using BlueBoxMoon.LocalSubway.Sessions;
using BlueBoxMoon.LocalSubway.Tunnels;

namespace BlueBoxMoon.LocalSubway.Cli.Tunnels
{
    /// <summary>
    /// A tunnel for TCP connections.
    /// </summary>
    /// <seealso cref="BlueBoxMoon.LocalSubway.Tunnels.ClientTunnel" />
    public class TcpClientTunnel : ClientTunnel
    {
        #region Fields

        /// <summary>
        /// The hostname
        /// </summary>
        private readonly string _hostname;

        /// <summary>
        /// The port
        /// </summary>
        private readonly int _port;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpClientTunnel"/> class.
        /// </summary>
        /// <param name="tunnelId">The tunnel identifier.</param>
        /// <param name="hostname">The hostname.</param>
        /// <param name="port">The port.</param>
        public TcpClientTunnel( Guid tunnelId, string hostname, int port )
            : base( tunnelId )
        {
            _hostname = hostname;
            _port = port;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new connection for the tunnel.
        /// </summary>
        /// <param name="session">The session that will own the connection.</param>
        /// <param name="connectionId"></param>
        /// <returns>
        /// The <see cref="T:BlueBoxMoon.LocalSubway.Connections.Connection" /> that can now be added to the tunnel.
        /// </returns>
        public override Task<Connection> CreateConnectionAsync( ClientSession session, Guid connectionId )
        {
            var socket = new SubwayTcpClient( _hostname, _port );

            Connection connection = new TcpClientConnection( connectionId, Id, session, socket );

            return Task.FromResult( connection );
        }

        #endregion
    }
}
