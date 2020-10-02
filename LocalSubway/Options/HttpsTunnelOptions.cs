using System;

namespace BlueBoxMoon.LocalSubway.Cli.Options
{
    /// <summary>
    /// The options for a web tunnel to an HTTPS server.
    /// </summary>
    /// <seealso cref="BlueBoxMoon.LocalSubway.Client.HttpTunnelOptions" />
    public class HttpsTunnelOptions : HttpTunnelOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpsTunnelOptions"/> class.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <exception cref="ArgumentException">Invalid URI for web tunnel. - arguments</exception>
        public HttpsTunnelOptions( string arguments )
            : base( arguments )
        {
            var segments = arguments.Split( ':' );

            if ( !Uri.TryCreate( $"https://{segments[1]}:{segments[2]}", UriKind.Absolute, out var uri ) )
            {
                throw new ArgumentException( "Invalid URI for web tunnel.", nameof( arguments ) );
            }

            Uri = uri;
        }
    }
}
