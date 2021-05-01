using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using BlueBoxMoon.LocalSubway.Connections;
using BlueBoxMoon.LocalSubway.Sessions;

namespace BlueBoxMoon.LocalSubway.Tunnels
{
    /// <summary>
    /// A tunnel that listens for new TCP connections.
    /// </summary>
    /// <seealso cref="BlueBoxMoon.LocalSubway.Tunnel" />
    public class TcpListenerTunnel : Tunnel
    {
        #region Properties

        /// <summary>
        /// Gets the session.
        /// </summary>
        /// <value>
        /// The session.
        /// </value>
        public ServerSession Session { get; }

        /// <summary>
        /// Gets the listener.
        /// </summary>
        /// <value>
        /// The listener.
        /// </value>
        public TcpListener Listener { get; }

        /// <summary>
        /// Gets the port being listened on.
        /// </summary>
        /// <value>
        /// The port being listened on.
        /// </value>
        public int Port { get; }

        /// <summary>
        /// Gets the cancellation token source.
        /// </summary>
        /// <value>
        /// The cancellation token source.
        /// </value>
        protected CancellationTokenSource CancellationTokenSource { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpTunnel"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="port">The port to listen on.</param>
        public TcpListenerTunnel( ServerSession session, int port )
        {
            CancellationTokenSource = new CancellationTokenSource();
            Session = session;
            Listener = new ReusableTcpListener( IPAddress.Any, port );
            Listener.Start();
            Port = ( ( IPEndPoint ) Listener.LocalEndpoint ).Port;

            _ = AcceptLoopAsync();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Accepts the new TCP connections until cancelled.
        /// </summary>
        private async Task AcceptLoopAsync()
        {
            while ( true )
            {
                TcpClient client;

                try
                {
                    client = await Listener.AcceptTcpClientAsync();
                }
                catch
                {
                    if ( !CancellationTokenSource.IsCancellationRequested )
                    {
                        await Session.RemoveTunnelAsync( this );
                    }

                    break;
                }

                var connection = new TcpServerConnection( Id, Session, client );

                try
                {
                    await Session.AddConnection( connection, CancellationTokenSource.Token );
                    await connection.StartAsync();
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine( "Failed to add connection for new TCP client." );
                }
            }
        }

        /// <summary>
        /// Closes the local side of the tunnel, for example a server might
        /// close a listening TCP port.
        /// </summary>
        public override Task CloseLocalAsync()
        {
            CancellationTokenSource.Cancel();

            Listener.Stop();

            return Task.CompletedTask;
        }

        #endregion
    }
}
