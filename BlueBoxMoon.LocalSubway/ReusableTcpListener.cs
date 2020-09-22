using System.Net;
using System.Net.Sockets;

namespace BlueBoxMoon.LocalSubway
{
    /// <summary>
    /// A <see cref="TcpListener"/> that enables the ReuseAddress option.
    /// </summary>
    /// <seealso cref="System.Net.Sockets.TcpListener" />
    public class ReusableTcpListener : TcpListener
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReusableTcpListener"/> class.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        public ReusableTcpListener( IPAddress address, int port )
            : base( address, port )
        {
            Server.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1 );
        }
    }
}
