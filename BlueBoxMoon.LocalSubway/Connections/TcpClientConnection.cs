﻿using System;
using System.Threading;
using System.Threading.Tasks;

using BlueBoxMoon.LocalSubway.Sessions;

namespace BlueBoxMoon.LocalSubway.Connections
{
    /// <summary>
    /// Handles the connection logic for a TCP connection.
    /// </summary>
    /// <seealso cref="BlueBoxMoon.LocalSubway.Connection" />
    public class TcpClientConnection : Connection
    {
        #region Properties

        /// <summary>
        /// Gets the session.
        /// </summary>
        /// <value>
        /// The session.
        /// </value>
        protected ClientSession Session { get; private set; }

        /// <summary>
        /// Gets the local socket.
        /// </summary>
        /// <value>
        /// The local socket.
        /// </value>
        protected SubwayTcpClient LocalSocket { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SubwayClientConnection"/> class.
        /// </summary>
        /// <param name="tunnelId">The tunnel identifier.</param>
        /// <param name="session">The session.</param>
        /// <param name="socket">The socket.</param>
        public TcpClientConnection( Guid id, Guid tunnelId, ClientSession session, SubwayTcpClient socket )
            : base( id, tunnelId )
        {
            Session = session;
            LocalSocket = socket;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the connection and begins reading and writing data.
        /// </summary>
        /// <returns></returns>
        public override Task StartAsync()
        {
            _ = ReadLocalSocketLoopAsync();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Runs the read loop for the local socket.
        /// </summary>
        private async Task ReadLocalSocketLoopAsync()
        {
            while ( true )
            {
                try
                {
                    var bytes = new byte[4096];

                    var count = await LocalSocket.GetStream().ReadAsync( bytes, 0, bytes.Length, CancellationToken.None );

                    if ( count == 0 )
                    {
                        await Session.RemoveConnectionAsync( this );
                        break;
                    }

                    await SendDataToRemoteAsync( new ArraySegment<byte>( bytes, 0, count ) );
                }
                catch
                {
                    if ( Session != null )
                    {
                        await Session.RemoveConnectionAsync( this );
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Sends the data to local side of the connection.
        /// </summary>
        /// <param name="data">The data to be sent.</param>
        public override Task SendDataToLocalAsync( ArraySegment<byte> data )
        {
            return LocalSocket.GetStream().WriteAsync( data.Array, data.Offset, data.Count, CancellationToken.None );
        }

        /// <summary>
        /// Closes the local side of the connection.
        /// </summary>
        public override Task CloseLocalAsync()
        {
            LocalSocket?.Close();
            LocalSocket = null;

            Session = null;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sends the data to remote session.
        /// </summary>
        /// <param name="data">The data.</param>
        protected virtual Task SendDataToRemoteAsync( ArraySegment<byte> data )
        {
            return Session.SendDataAsync( Id, data );
        }

        #endregion
    }
}
