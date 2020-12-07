using System;

namespace BlueBoxMoon.LocalSubway.Cli.Options
{
    /// <summary>
    /// The options for a simple TCP echo port.
    /// </summary>
    public class EchoTunnelOptions
    {
        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>
        /// The port.
        /// </value>
        public int Port { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EchoTunnelOptions"/> class.
        /// </summary>
        public EchoTunnelOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EchoTunnelOptions"/> class.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <exception cref="ArgumentException">Invalid port for echo tunnel. - arguments</exception>
        public EchoTunnelOptions( string arguments )
        {
            if ( !int.TryParse( arguments, out var port ) )
            {
                throw new ArgumentException( "Invalid port for echo tunnel.", nameof( arguments ) );
            }

            Port = port;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Port.ToString();
        }
    }
}
