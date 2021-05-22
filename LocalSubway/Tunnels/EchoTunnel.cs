using System;
using System.Threading.Tasks;

using BlueBoxMoon.LocalSubway.Connections;
using BlueBoxMoon.LocalSubway.Sessions;
using BlueBoxMoon.LocalSubway.Tunnels;

namespace BlueBoxMoon.LocalSubway.Cli.Tunnels
{
    /// <summary>
    /// A tunnel that simply creates echo connections back to the sender.
    /// </summary>
    /// <seealso cref="BlueBoxMoon.LocalSubway.ClientTunnel" />
    public class EchoTunnel : ClientTunnel
    {
        public EchoTunnel( Guid id )
            : base( id )
        {
        }

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
            var connection = new EchoConnection( session, connectionId, Id );

            await connection.StartAsync();

            return connection;
        }
    }
}
