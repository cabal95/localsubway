using System;
using System.Collections.Generic;

using CommandLine;
using CommandLine.Text;

namespace BlueBoxMoon.LocalSubway.Cli.Options
{
    /// <summary>
    /// The options specified by the user.
    /// </summary>
    public class ProgramOptions
    {
        [Option( 's', "server", Required = false, HelpText = "Defines the server URI to use when opening the tunnels (Default is https://subwayapp.dev)." )]
        public Uri Server { get; set; }

        [Option( 'e', "echo", Required = false, HelpText = "Creates a simple TCP tunnel that echoes back whatever it receives." )]
        public IEnumerable<EchoTunnelOptions> EchoTunnels { get; set; }

        [Option( 't', "tcp", Required = false, HelpText = "Creates a TCP tunnel that routes the remote listening port to the specified host and port." )]
        public IEnumerable<TcpTunnelOptions> TcpTunnels { get; set; }

        [Option( 'h', "http", Required = false, HelpText = "Creates a web tunnel that routes the requested subdomain to the specified HTTP server." )]
        public IEnumerable<HttpTunnelOptions> HttpTunnels { get; set; }

        [Option( 'H', "https", Required = false, HelpText = "Creates a web tunnel that routes the requested subdomain to the specified HTTPS server." )]
        public IEnumerable<HttpsTunnelOptions> HttpsTunnels { get; set; }

        [Usage( ApplicationAlias = "dotnet localsubway" )]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example( "TCP Tunnel", new ProgramOptions { TcpTunnels = new List<TcpTunnelOptions> { new TcpTunnelOptions( "5100:localhost:5000" ) } } );
                yield return new Example( "HTTP Tunnel", new ProgramOptions { HttpTunnels = new List<HttpTunnelOptions> { new HttpTunnelOptions( "5100:localhost:5000" ) } } );
                yield return new Example( "HTTPS Tunnel", new ProgramOptions { HttpsTunnels = new List<HttpsTunnelOptions> { new HttpsTunnelOptions( "5100:localhost:5000" ) } } );
                yield return new Example( "Echo Tunnel", new ProgramOptions { EchoTunnels = new List<EchoTunnelOptions> { new EchoTunnelOptions( "5100" ) } } );
            }
        }
    }
}
