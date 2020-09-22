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
    public class ServerSession : Session
    {
        #region Properties

        /// <summary>
        /// Gets the tunnels that are active.
        /// </summary>
        /// <value>
        /// The tunnels that are active.
        /// </value>
        private ConcurrentDictionary<Guid, Tunnel> Tunnels { get; } = new ConcurrentDictionary<Guid, Tunnel>();

        /// <summary>
        /// Gets the connections that are active.
        /// </summary>
        /// <value>
        /// The connections that are active.
        /// </value>
        private ConcurrentDictionary<Guid, Connection> Connections { get; } = new ConcurrentDictionary<Guid, Connection>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSession"/> class.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="jsonConverter">The json converter.</param>
        public ServerSession( WebSocket socket, IJsonConverter jsonConverter )
            : base( socket, jsonConverter )
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a new connection to the session.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="token">The token.</param>
        /// <exception cref="System.InvalidOperationException">Can not add connection to unknown tunnel.</exception>
        public virtual async Task AddConnection( Connection connection, CancellationToken token )
        {
            if ( !Tunnels.TryGetValue( connection.TunnelId, out var _ ) )
            {
                throw new InvalidOperationException( "Can not add connection to unknown tunnel." );
            }

            Connections.TryAdd( connection.Id, connection );

            var message = new Message
            {
                Type = MessageType.Message,
                Code = MessageCode.NewConnectionMessage,
            };

            message.Values["connection_id"] = connection.Id;
            message.Values["tunnel_id"] = connection.TunnelId;

            var response = await SendMessageAsync( message, token );

            if ( !response.Success )
            {
                throw new Exception( "Could not add connection." );
            }
        }

        /// <summary>
        /// Server initiated connection closure.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public virtual async Task RemoveConnectionAsync( Connection connection )
        {
            if ( Connections.TryRemove( connection.Id, out var _ ) )
            {
                var message = new Message
                {
                    Type = MessageType.Notification,
                    Code = MessageCode.ConnectionClosedNotification
                };

                message.Values["connection_id"] = connection.Id;

                SendNotification( message );

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
        public virtual void AddTunnel( Tunnel tunnel )
        {
            Tunnels.TryAdd( tunnel.Id, tunnel );
        }

        /// <summary>
        /// Removes the tunnel from the server and closes all connections.
        /// </summary>
        /// <param name="tunnel">The tunnel to be removed.</param>
        public virtual Task RemoveTunnelAsync( Tunnel tunnel )
        {
            return RemoveTunnelAsync( tunnel, true );
        }

        /// <summary>
        /// Removes the tunnel from the server and closes all connections.
        /// </summary>
        /// <param name="tunnel">The tunnel to be removed.</param>
        /// <param name="notifyClient">if set to <c>true</c> then notify the client of the closure.</param>
        protected virtual async Task RemoveTunnelAsync( Tunnel tunnel, bool notifyClient )
        {
            var connections = Connections.Values.Where( a => a.TunnelId == tunnel.Id ).ToList();

            foreach ( var connection in connections )
            {
                try
                {
                    if ( Connections.TryRemove( connection.Id, out var _ ) )
                    {
                        await connection.CloseLocalAsync();
                    }
                }
                catch
                {
                    /* Intentionally ignored so that we close all connections. */
                }
            }

            await tunnel.CloseLocalAsync();

            if ( notifyClient )
            {
                var message = new Message
                {
                    Type = MessageType.Notification,
                    Code = MessageCode.TunnelClosedNotification
                };

                message.Values["tunnel_id"] = tunnel.Id;

                SendNotification( message );
            }
        }

        /// <summary>
        /// Processes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        protected override async Task ProcessMessage( Message message )
        {
            switch (message.Code)
            {
                case MessageCode.CreateWebTunnelMessage:
                    SendResponse( await CreateWebTunnelAsync( message.Id, message.Values["domain"].ToString() ) );
                    break;

                case MessageCode.CreateTcpTunnelMessage:
                    SendResponse( await CreateTcpTunnelAsync( message.Id, message.Values["port"].ToString().AsInteger() ) );
                    break;

                case MessageCode.CloseTunnelMessage:
                    SendResponse( await CloseTunnelAsync( message.Id, message.Values["tunnel_id"].ToString().AsGuid() ) );
                    break;

                case MessageCode.CloseConnectionMessage:
                    SendResponse( await CloseConnectionAsync( message.Id, message.Values["connection_id"].ToString().AsGuid() ) );
                    break;

                default:
                    SendResponse( new Response( message.Id, false, "Unknown command code." ) );
                    break;
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
        /// Creates a new web tunnel to the client.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="domain">The domain to be used for the tunnel.</param>
        /// <returns>A <see cref="Response"/> to be sent back to the client.</returns>
        /// <exception cref="NotSupportedException">CreateWebTunnel message is not supported.</exception>
        protected virtual Task<Response> CreateWebTunnelAsync( Guid messageId, string domain )
        {
            throw new NotSupportedException( "CreateWebTunnel message is not supported." );
        }

        /// <summary>
        /// Creates a new TCP tunnel to the client.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="port">The port to listen on.</param>
        /// <returns>A <see cref="Response"/> to be sent back to the client.</returns>
        /// <exception cref="NotSupportedException">CreateTcpTunnel message is not supported.</exception>
        protected virtual Task<Response> CreateTcpTunnelAsync( Guid messageId, int port )
        {
            throw new NotSupportedException( "CreateTcpTunnel message is not supported." );
        }

        /// <summary>
        /// Closes the requested tunnel and all outstanding connections.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="tunnelId">The tunnel identifier.</param>
        /// <returns>A <see cref="Response"/> to be sent back to the client.</returns>
        protected virtual async Task<Response> CloseTunnelAsync( Guid messageId, Guid tunnelId )
        {
            if ( Tunnels.TryRemove( tunnelId, out var tunnel ) )
            {
                try
                {
                    await RemoveTunnelAsync( tunnel, false );
                }
                catch ( Exception ex )
                {
                    return new Response( messageId, false, ex.Message );
                }

                return new Response( messageId, true, "Tunnel closed." );
            }
            else
            {
                return new Response( messageId, false, "Tunnel not found." );
            }
        }

        /// <summary>
        /// Client is requesting that a connection be closed.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <returns>A <see cref="Response"/> to be sent back to the client.</returns>
        protected virtual async Task<Response> CloseConnectionAsync( Guid messageId, Guid connectionId )
        {
            if ( Connections.TryRemove( connectionId, out var connection ) )
            {
                try
                {
                    //
                    // Don't send a notification since it's the client requesting the closure.
                    //
                    await connection.CloseLocalAsync();

                    return new Response( messageId, true, "Connection closed." );
                }
                catch ( Exception ex )
                {
                    return new Response( messageId, false, ex.Message );
                }
            }
            else
            {
                return new Response( messageId, false, "Connection not found." );
            }
        }

        /// <summary>
        /// Closes this session.
        /// </summary>
        public override async Task CloseAsync()
        {
            //
            // Make sure the socket is closed first so no new tunnels are created.
            //
            await base.CloseAsync();

            var tunnels = Tunnels.Values;

            foreach ( var tunnel in tunnels )
            {
                await RemoveTunnelAsync( tunnel, false );
            }
        }

        #endregion
    }
}
