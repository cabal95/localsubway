using System;
using System.Threading.Tasks;

using BlueBoxMoon.LocalSubway.Cli.Connections;
using BlueBoxMoon.LocalSubway.Connections;
using BlueBoxMoon.LocalSubway.Sessions;
using BlueBoxMoon.LocalSubway.Tunnels;

namespace BlueBoxMoon.LocalSubway.Cli.Tunnels
{
    /// <summary>
    /// A tunnel that connects to a local web service.
    /// </summary>
    /// <seealso cref="BlueBoxMoon.LocalSubway.ClientTunnel" />
    public class WebTunnel : ClientTunnel
    {
        #region Fields

        /// <summary>
        /// The hostname to connect to.
        /// </summary>
        private readonly string _hostname;

        /// <summary>
        /// The port to connect to.
        /// </summary>
        private readonly int _port;

        /// <summary>
        /// If <c>true</c> then use an SSL connection.
        /// </summary>
        private readonly bool _useSsl;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WebTunnel" /> class.
        /// </summary>
        /// <param name="id">The tunnel identifier.</param>
        /// <param name="hostname">The hostname to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        /// <param name="useSsl">if set to <c>true</c> then use SSL.</param>
        public WebTunnel( Guid id, string hostname, int port, bool useSsl )
            : base( id )
        {
            _hostname = hostname;
            _port = port;
            _useSsl = useSsl;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new connection for the tunnel.
        /// </summary>
        /// <param name="session">The session that will own the connection.</param>
        /// <param name="connectionId"></param>
        /// <returns>
        /// The <see cref="T:BlueBoxMoon.LocalSubway.Connection" /> that can now be added to the tunnel.
        /// </returns>
        public override async Task<Connection> CreateConnectionAsync( ClientSession session, Guid connectionId )
        {
            SubwayTcpClient socket;

            if ( _useSsl )
            {
                socket = new SubwaySslClient( _hostname, _port );
            }
            else
            {
                socket = new SubwayTcpClient( _hostname, _port );
            }

            var connection = new WebClientConnection( connectionId, Id, session, socket, _hostname );
            await connection.StartAsync();

            return connection;
        }

        #endregion
    }
}
