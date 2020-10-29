using System;

namespace BlueBoxMoon.LocalSubway.Cli.Options
{
    /// <summary>
    /// The options for a web tunnel to an HTTP server.
    /// </summary>
    public class HttpTunnelOptions
    {
        /// <summary>
        /// Gets or sets the subdomain.
        /// </summary>
        /// <value>
        /// The subdomain.
        /// </value>
        public string Subdomain { get; set; }

        /// <summary>
        /// Gets or sets the URI.
        /// </summary>
        /// <value>
        /// The URI.
        /// </value>
        public Uri Uri { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpTunnelOptions"/> class.
        /// </summary>
        public HttpTunnelOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpTunnelOptions"/> class.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <exception cref="ArgumentException">
        /// Invalid syntax for web tunnel. - arguments
        /// or
        /// Invalid URI for web tunnel. - arguments
        /// </exception>
        public HttpTunnelOptions( string arguments )
        {
            var segments = arguments.Split( ':' );

            if ( segments.Length != 3 )
            {
                throw new ArgumentException( "Invalid syntax for web tunnel.", nameof( arguments ) );
            }

            if ( !Uri.TryCreate( $"http://{segments[1]}:{segments[2]}", UriKind.Absolute, out var uri ) )
            {
                throw new ArgumentException( "Invalid URI for web tunnel.", nameof( arguments ) );
            }

            Subdomain = segments[0];
            Uri = uri;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{Subdomain}:{Uri.Host}:{Uri.Port}";
        }
    }
}
