using System;
using System.Collections.Generic;
using System.Linq;

using CommandLine;
using CommandLine.Text;

namespace BlueBoxMoon.LocalSubway.Cli.Options
{
    /// <summary>
    /// The options specified by the user.
    /// </summary>
    public class ProgramOptions
    {
        /// <summary>
        /// The server URL to connect to when creating the tunnel.
        /// </summary>
        [Option( 's', "server", Required = false, HelpText = "Defines the server URI to use when opening the tunnels (Default is https://subwayapp.dev)." )]
        public Uri Server { get; set; }

        /// <summary>
        /// The API key to use if the server requires one.
        /// </summary>
        [Option( 'k', "key", Required = false, HelpText = "Specifies the authentication token to use if your server requires it." )]
        public string Key { get; set; }

        /// <summary>
        /// If an error occurs then keep trying to reconnect.
        /// </summary>
        [Option( 'f', "forever", Required = false, HelpText = "If set and an error occurs then keep trying to reconnect.")]
        public bool Forever { get; set; }

        /// <summary>
        /// A collection of Echo tunnel configuration options.
        /// </summary>
        [Option( 'e', "echo", Required = false, HelpText = "Creates a simple TCP tunnel that echoes back whatever it receives." )]
        public IEnumerable<EchoTunnelOptions> EchoTunnels { get; set; } = new List<EchoTunnelOptions>();

        /// <summary>
        /// A collection of TCP tunnel configuration options.
        /// </summary>
        [Option( 't', "tcp", Required = false, HelpText = "Creates a TCP tunnel that routes the remote listening port to the specified host and port." )]
        public IEnumerable<TcpTunnelOptions> TcpTunnels { get; set; } = new List<TcpTunnelOptions>();

        /// <summary>
        /// A collection of HTTP tunnel configuration options.
        /// </summary>
        [Option( 'h', "http", Required = false, HelpText = "Creates a web tunnel that routes the requested subdomain to the specified HTTP server." )]
        public IEnumerable<HttpTunnelOptions> HttpTunnels { get; set; } = new List<HttpTunnelOptions>();

        /// <summary>
        /// A collection of HTTPS tunnel configuration options.
        /// </summary>
        [Option( 'H', "https", Required = false, HelpText = "Creates a web tunnel that routes the requested subdomain to the specified HTTPS server." )]
        public IEnumerable<HttpsTunnelOptions> HttpsTunnels { get; set; } = new List<HttpsTunnelOptions>();

        /// <summary>
        /// A collection of examples to display.
        /// </summary>
        [Usage( ApplicationAlias = "dotnet localsubway" )]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example( "TCP Tunnel", new ProgramOptions { TcpTunnels = new List<TcpTunnelOptions> { new TcpTunnelOptions( "5100:localhost:5000" ) } } );
                yield return new Example( "HTTP Tunnel", new ProgramOptions { HttpTunnels = new List<HttpTunnelOptions> { new HttpTunnelOptions( "mydomain:localhost:5000" ) } } );
                yield return new Example( "HTTPS Tunnel", new ProgramOptions { HttpsTunnels = new List<HttpsTunnelOptions> { new HttpsTunnelOptions( "mydomain:localhost:5001" ) } } );
                yield return new Example( "Echo Tunnel", new ProgramOptions { EchoTunnels = new List<EchoTunnelOptions> { new EchoTunnelOptions( "5100" ) } } );
            }
        }

        /// <summary>
        /// Merges the options from <paramref name="otherOptions"/> into these options.
        /// </summary>
        /// <param name="otherOptions">The other options.</param>
        public void MergeOptionsFrom( ProgramOptions otherOptions )
        {
            if ( otherOptions.Server != null )
            {
                Server = otherOptions.Server;
            }

            if ( otherOptions.Key != null && otherOptions.Key.Length > 0 )
            {
                Key = otherOptions.Key;
            }

            if ( otherOptions.Forever )
            {
                Forever = true;
            }

            //
            // Merge any defined echo tunnels.
            //
            if ( otherOptions.EchoTunnels != null && otherOptions.EchoTunnels.Any() )
            {
                if ( EchoTunnels == null )
                {
                    EchoTunnels = otherOptions.EchoTunnels;
                }
                else
                {
                    EchoTunnels = new List<EchoTunnelOptions>( EchoTunnels.Concat( otherOptions.EchoTunnels ) );
                }
            }

            //
            // Merge any defined TCP tunnels.
            //
            if ( otherOptions.TcpTunnels != null && otherOptions.TcpTunnels.Any() )
            {
                if ( TcpTunnels == null )
                {
                    TcpTunnels = otherOptions.TcpTunnels;
                }
                else
                {
                    TcpTunnels = new List<TcpTunnelOptions>( TcpTunnels.Concat( otherOptions.TcpTunnels ) );
                }
            }

            //
            // Merge any defined HTTP tunnels.
            //
            if ( otherOptions.HttpTunnels != null && otherOptions.HttpTunnels.Any() )
            {
                if ( HttpTunnels == null )
                {
                    HttpTunnels = otherOptions.HttpTunnels;
                }
                else
                {
                    HttpTunnels = new List<HttpTunnelOptions>( HttpTunnels.Concat( otherOptions.HttpTunnels ) );
                }
            }

            //
            // Merge any defined HTTPS tunnels.
            //
            if ( otherOptions.HttpsTunnels != null && otherOptions.HttpsTunnels.Any() )
            {
                if ( HttpsTunnels == null )
                {
                    HttpsTunnels = otherOptions.HttpsTunnels;
                }
                else
                {
                    HttpsTunnels = new List<HttpsTunnelOptions>( HttpsTunnels.Concat( otherOptions.HttpsTunnels ) );
                }
            }
        }
    }
}
