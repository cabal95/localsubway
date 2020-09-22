using System;
using System.Threading;
using System.Threading.Tasks;

using BlueBoxMoon.LocalSubway.Connections;
using BlueBoxMoon.LocalSubway.Sessions;

namespace BlueBoxMoon.LocalSubway.Cli.Connections
{
    /// <summary>
    /// A connection to a local web server with optional header rewriting.
    /// </summary>
    /// <seealso cref="BlueBoxMoon.LocalSubway.Connections.TcpClientConnection" />
    public class WebClientConnection : TcpClientConnection
    {
        #region Fields

        /// <summary>
        /// The interceptor handling parsing HTTP headers.
        /// </summary>
        private HttpWebRequestInterceptor _interceptor;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WebClientConnection"/> class.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="tunnelId">The tunnel identifier.</param>
        /// <param name="session">The session.</param>
        /// <param name="socket">The socket.</param>
        public WebClientConnection( Guid connectionId, Guid tunnelId, ClientSession session, SubwayTcpClient socket, string hostHeader )
            : base( connectionId, tunnelId, session, socket )
        {
            _interceptor = new HttpWebRequestInterceptor( socket.GetStream() );

            Console.WriteLine( "hostHEader = " + hostHeader );
            if ( hostHeader != null )
            {
                _interceptor.ForcedHeaders.Add( "Host", hostHeader );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sends the data to local side of the connection.
        /// </summary>
        /// <param name="data">The data to be sent.</param>
        public override Task SendDataToLocalAsync( ArraySegment<byte> data )
        {
            if ( _interceptor != null )
            {
                return _interceptor.WriteAsync( data.Array, data.Offset, data.Count, CancellationToken.None );
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Closes the local side of the connection.
        /// </summary>
        /// <returns></returns>
        public override Task CloseLocalAsync()
        {
            if ( _interceptor != null )
            {
                _interceptor.Dispose();
                _interceptor = null;
            }

            return base.CloseLocalAsync();
        }

        #endregion
    }
}
