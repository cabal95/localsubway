using System;

namespace BlueBoxMoon.LocalSubway.Cli.Options
{
    /// <summary>
    /// The options for a TCP tunnel to a local server.
    /// </summary>
    public class TcpTunnelOptions 
    {
        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>
        /// The port.
        /// </value>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the target hostname.
        /// </summary>
        /// <value>
        /// The target hostname.
        /// </value>
        public string TargetHostname { get; set; }

        /// <summary>
        /// Gets or sets the target port.
        /// </summary>
        /// <value>
        /// The target port.
        /// </value>
        public int TargetPort { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpTunnelOptions"/> class.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <exception cref="ArgumentException">
        /// Invalid syntax for TCP tunnel. - arguments
        /// or
        /// Invalid port for TCP tunnel. - arguments
        /// or
        /// Invalid target port for TCP tunnel. - arguments
        /// </exception>
        public TcpTunnelOptions( string arguments )
        {
            var segments = arguments.Split( ':' );

            if ( segments.Length != 3 )
            {
                throw new ArgumentException( "Invalid syntax for TCP tunnel.", nameof( arguments ) );
            }

            if ( !int.TryParse( segments[0], out var port ) )
            {
                throw new ArgumentException( "Invalid port for TCP tunnel.", nameof( arguments ) );
            }

            if ( !int.TryParse( segments[2], out var targetPort ) )
            {
                throw new ArgumentException( "Invalid target port for TCP tunnel.", nameof( arguments ) );
            }

            Port = port;
            TargetHostname = segments[1];
            TargetPort = targetPort;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{Port}:{TargetHostname}:{TargetPort}";
        }
    }
}
