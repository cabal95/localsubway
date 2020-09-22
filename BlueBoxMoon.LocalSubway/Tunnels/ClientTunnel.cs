using System;
using System.Threading.Tasks;

using BlueBoxMoon.LocalSubway.Connections;
using BlueBoxMoon.LocalSubway.Sessions;

namespace BlueBoxMoon.LocalSubway.Tunnels
{
    /// <summary>
    /// A tunnel that is used by the client to handle connections.
    /// </summary>
    /// <seealso cref="BlueBoxMoon.LocalSubway.Tunnel" />
    public abstract class ClientTunnel : Tunnel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientTunnel"/> class.
        /// </summary>
        /// <param name="id">The tunnel identifier.</param>
        public ClientTunnel( Guid id )
            : base( id )
        {
        }

        /// <summary>
        /// Creates a new connection for the tunnel.
        /// </summary>
        /// <param name="session">The session that will own the connection.</param>
        /// <param name="id">The connection identifier.</param>
        /// <returns>
        /// The <see cref="Connection" /> that can now be added to the tunnel.
        /// </returns>
        public abstract Task<Connection> CreateConnectionAsync( ClientSession session, Guid connectionId );
    }
}
