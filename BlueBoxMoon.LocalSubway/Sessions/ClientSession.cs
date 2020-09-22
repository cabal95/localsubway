using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using BlueBoxMoon.LocalSubway.Connections;
using BlueBoxMoon.LocalSubway.Messages;
using BlueBoxMoon.LocalSubway.Tunnels;

namespace BlueBoxMoon.LocalSubway.Sessions
{
    /// <summary>
    /// A base client session that will handle the communication with a remote
    /// server session.
    /// </summary>
    /// <seealso cref="BlueBoxMoon.LocalSubway.Session" />
    public class ClientSession : Session
    {
        #region Properties

        /// <summary>
        /// Gets the tunnels that are active.
        /// </summary>
        /// <value>
        /// The tunnels that are active.
        /// </value>
        protected ConcurrentDictionary<Guid, ClientTunnel> Tunnels { get; } = new ConcurrentDictionary<Guid, ClientTunnel>();

        /// <summary>
        /// Gets the connections that are active.
        /// </summary>
        /// <value>
        /// The connections that are active.
        /// </value>
        protected ConcurrentDictionary<Guid, Connection> Connections { get; } = new ConcurrentDictionary<Guid, Connection>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSession"/> class.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="jsonConverter">The json converter.</param>
        public ClientSession( WebSocket socket, IJsonConverter jsonConverter )
            : base( socket, jsonConverter )
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a new connection to the session.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public virtual void AddConnection( Connection connection )
        {
            if ( !Tunnels.TryGetValue( connection.TunnelId, out var _ ) )
            {
                throw new InvalidOperationException( "Can not add connection to unknown tunnel." );
            }

            Connections.TryAdd( connection.Id, connection );
        }

        /// <summary>
        /// Client initiated connection closure.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public virtual Task RemoveConnectionAsync( Connection connection )
        {
            return RemoveConnectionAsync( connection, true );
        }

        /// <summary>
        /// Client initiated connection closure.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="notifyServer">if set to <c>true</c> the server should be notified.</param>
        public virtual async Task RemoveConnectionAsync( Connection connection, bool notifyServer )
        {
            if ( Connections.TryRemove( connection.Id, out var _ ) )
            {
                if ( notifyServer )
                {
                    var message = new Message
                    {
                        Type = MessageType.Message,
                        Code = MessageCode.CloseConnectionMessage
                    };

                    message.Values["connection_id"] = connection.Id;

                    await SendMessageAsync( message, CancellationToken.None );
                }

                try
                {
                    await connection.CloseLocalAsync();
                }
                catch
                {
                    /* Intentionally ignored, we don't care if it can't actually close. */
                }
            }
        }

        /// <summary>
        /// Adds the tunnel.
        /// </summary>
        /// <param name="tunnel">The tunnel.</param>
        public virtual void AddTunnel( ClientTunnel tunnel )
        {
            Tunnels.TryAdd( tunnel.Id, tunnel );
        }

        /// <summary>
        /// Removes the tunnel from the server and closes all connections.
        /// </summary>
        /// <param name="tunnel">The tunnel to be removed.</param>
        public virtual Task RemoveTunnelAsync( ClientTunnel tunnel )
        {
            return RemoveTunnelAsync( tunnel, true );
        }

        /// <summary>
        /// Removes the tunnel from the server and closes all connections.
        /// </summary>
        /// <param name="tunnel">The tunnel to be removed.</param>
        /// <param name="notifyServer">if set to <c>true</c> then notify the server of the closure.</param>
        protected virtual async Task RemoveTunnelAsync( ClientTunnel tunnel, bool notifyServer )
        {
            if ( Tunnels.TryRemove( tunnel.Id, out var _ ) )
            {
                if ( notifyServer )
                {
                    var message = new Message
                    {
                        Type = MessageType.Notification,
                        Code = MessageCode.CloseTunnelMessage
                    };

                    message.Values["tunnel_id"] = tunnel.Id;

                    await SendMessageAsync( message, CancellationToken.None );
                }

                var connections = Connections.Values.Where( a => a.TunnelId == tunnel.Id ).ToList();

                foreach ( var connection in connections )
                {
                    await RemoveConnectionAsync( connection, false );
                }

                await tunnel.CloseLocalAsync();
            }
        }

        /// <summary>
        /// Processes the data message by asking the connection to handle it.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="data">The data.</param>
        protected override async Task ProcessData( Guid connectionId, ArraySegment<byte> data )
        {
            if ( Connections.TryGetValue( connectionId, out var connection ) )
            {
                await connection.SendDataToLocalAsync( data );
            }
        }

        /// <summary>
        /// Processes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        protected override async Task ProcessMessage( Message message )
        {
            switch ( message.Code )
            {
                case MessageCode.NewConnectionMessage:
                    SendResponse( await NewConnectionAsync( message.Id, message.Values["tunnel_id"].ToString().AsGuid(), message.Values["connection_id"].ToString().AsGuid() ) );
                    break;

                case MessageCode.ConnectionClosedNotification:
                    await ConnectionClosedAsync( message.Values["connection_id"].ToString().AsGuid() );
                    break;

                case MessageCode.TunnelClosedNotification:
                    await TunnelClosedAsync( message.Values["tunnel_id"].ToString().AsGuid() );
                    break;

                default:
                    /* TODO: Unknown message from server, maybe we should close? */
                    break;
            }
        }

        /// <summary>
        /// A new connection has been initiated for the given tunnel.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="tunnelId">The tunnel identifier.</param>
        /// <param name="connectionId">The connection identifier.</param>
        /// <returns></returns>
        protected virtual async Task<Response> NewConnectionAsync( Guid messageId, Guid tunnelId, Guid connectionId )
        {
            try
            {
                if ( Tunnels.TryGetValue( tunnelId, out var tunnel ) )
                {
                    var connection = await tunnel.CreateConnectionAsync( this, connectionId );

                    AddConnection( connection );

                    return new Response( messageId, true, "Connection ready." );
                }
                else
                {
                    return new Response( messageId, false, "Tunnel not found." );
                }
            }
            catch ( Exception ex )
            {
                return new Response( messageId, false, ex.Message );
            }
        }

        /// <summary>
        /// A connection has been closed by the server.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        protected virtual async Task ConnectionClosedAsync( Guid connectionId )
        {
            if ( Connections.TryGetValue( connectionId, out var connection ) )
            {
                try
                {
                    await RemoveConnectionAsync( connection, false );
                }
                catch ( Exception ex )
                {
                    System.Diagnostics.Debug.WriteLine( $"Error closing local connection: {ex.Message}" );
                }
            }
        }

        /// <summary>
        /// A tunnel has been closed by the server.
        /// </summary>
        /// <param name="tunnelId">The tunnel identifier.</param>
        protected virtual async Task TunnelClosedAsync( Guid tunnelId )
        {
            if ( Tunnels.TryGetValue( tunnelId, out var tunnel ) )
            {
                try
                {
                    await RemoveTunnelAsync( tunnel, false );
                }
                catch ( Exception ex )
                {
                    System.Diagnostics.Debug.WriteLine( $"Error closing local tunnel: {ex.Message}" );
                }
            }
        }

        #endregion
    }
}
