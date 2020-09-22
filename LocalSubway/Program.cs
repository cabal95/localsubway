using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using BlueBoxMoon.LocalSubway.Client.Tunnels;
using BlueBoxMoon.LocalSubway.Messages;

namespace BlueBoxMoon.LocalSubway.Client
{
    public interface ITunnelOptions
    {
    }

    public class EchoTunnelOptions : ITunnelOptions
    {
        public int Port { get; set; }
    }

    public class WebTunnelOptions : ITunnelOptions
    {
        public string Subdomain { get; set; }

        public Uri Uri { get; set; }
    }

    public class TcpTunnelOptions : ITunnelOptions
    {
        public int Port { get; set; }

        public string TargetHostname { get; set; }

        public int TargetPort { get; set; }
    }

    class Program
    {
        // --web subdomain,http://localhost:6229 --tcp 8022,localhost,22
        static async Task Main( string[] args )
        {
            var options = ParseOptions( args );

            if ( options == null )
            {
                return;
            }

            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += ( sender, e ) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            try
            {
                await RunAsync( options, cts.Token );
            }
            catch ( TaskCanceledException )
            {
                /* Intentionally ignored, the user requested the cancel. */
            }
        }

        /// <summary>
        /// Parses the options.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        private static ICollection<ITunnelOptions> ParseOptions( string[] args )
        {
            List<ITunnelOptions> tunnelOptions = new List<ITunnelOptions>();

            for ( int i = 0; i < args.Length; i++ )
            {
                if ( args[i] == "--web" )
                {
                    i++;
                    if ( i >= args.Length )
                    {
                        Console.WriteLine( "Missing argument for --web" );
                        return null;
                    }

                    var segments = args[i].Split( ',' );

                    if ( segments.Length != 2 )
                    {
                        Console.WriteLine( "Invalid syntax for web tunnel." );
                        return null;
                    }

                    if ( !Uri.TryCreate( segments[1], UriKind.Absolute, out var uri ) )
                    {
                        Console.WriteLine( "Invalid URI for web tunnel." );
                        return null;
                    }

                    if ( uri.Scheme != "http" && uri.Scheme != "https" )
                    {
                        Console.WriteLine( "Only http and https are supported for web tunnels." );
                        return null;
                    }

                    if ( uri.PathAndQuery != "/" )
                    {
                        Console.WriteLine( "Paths are not supported in web tunnels." );
                        return null;
                    }

                    tunnelOptions.Add( new WebTunnelOptions
                    {
                        Subdomain = segments[0],
                        Uri = uri
                    } );
                }
                else if ( args[i] == "--tcp" )
                {
                    i++;
                    if ( i >= args.Length )
                    {
                        Console.WriteLine( "Missing argument for --tcp" );
                        return null;
                    }

                    var segments = args[i].Split( ',' );

                    if ( segments.Length != 3 )
                    {
                        Console.WriteLine( "Invalid syntax for TCP tunnel." );
                        return null;
                    }

                    if ( !int.TryParse( segments[0], out var port ) )
                    {
                        Console.WriteLine( "Invalid port for TCP tunnel." );
                        return null;
                    }

                    if ( !int.TryParse( segments[2], out var targetPort ) )
                    {
                        Console.WriteLine( "Invalid target port for TCP tunnel." );
                        return null;
                    }


                    tunnelOptions.Add( new TcpTunnelOptions
                    {
                        Port = port,
                        TargetHostname = segments[1],
                        TargetPort = targetPort
                    } );
                }
                else if ( args[i] == "--echo" )
                {
                    i++;
                    if ( i >= args.Length )
                    {
                        Console.WriteLine( "Missing argument for --echo" );
                        return null;
                    }

                    if ( !int.TryParse( args[i], out var port ) )
                    {
                        Console.WriteLine( "Invalid port for echo tunnel." );
                        return null;
                    }

                    tunnelOptions.Add( new EchoTunnelOptions
                    {
                        Port = port
                    } );
                }
            }

            return tunnelOptions;
        }

        /// <summary>
        /// Runs the session.
        /// </summary>
        /// <param name="tunnelOptions">The tunnel options.</param>
        /// <param name="token">The cancellation token.</param>
        private static async Task RunAsync( ICollection<ITunnelOptions> tunnelOptions, CancellationToken token )
        {
            var ws = new ClientWebSocket();

            await ws.ConnectAsync( new Uri( "wss://localhost:5001/connect" ), token );

            var session = new SubwayClientSession( ws );

            var sessionTask = session.RunAsync( token );

            if ( !await SetupTunnelsAsync( session, tunnelOptions, token ) )
            {
                return;
            }

            await sessionTask;
        }

        /// <summary>
        /// Setups the tunnels.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="tunnelOptions">The tunnel options.</param>
        /// <param name="token">The token.</param>
        /// <returns><c>true</c> if the tunnels were correctly setup.</returns>
        private static async Task<bool> SetupTunnelsAsync( SubwayClientSession session, ICollection<ITunnelOptions> tunnelOptions, CancellationToken token )
        {
            if ( tunnelOptions.Count == 0 )
            {
                Console.WriteLine( "No tunnels were defined." );
                return false;
            }

            foreach ( var options in tunnelOptions )
            {
                if ( options is WebTunnelOptions webOptions )
                {
                    var message = new Message
                    {
                        Type = MessageType.Message,
                        Code = MessageCode.CreateWebTunnelMessage
                    };
                    message.Values["domain"] = webOptions.Subdomain;

                    var response = await session.SendMessageAsync( message, token );

                    if ( !response.Success )
                    {
                        Console.WriteLine( $"Failed to initialize web tunnel for '{webOptions.Subdomain}': {response.Message}" );
                        return false;
                    }

                    var tunnelId = response.Values["tunnel_id"].ToString().AsGuid();
                    var tunnelUrl = response.Values["url"].ToString();

                    session.AddTunnel( new WebTunnel( tunnelId, webOptions.Uri.Host, webOptions.Uri.Port, webOptions.Uri.Scheme == "https" ) );

                    Console.WriteLine( $"Web Tunnel {tunnelUrl} => {webOptions.Uri} ready." );
                }
                else if ( options is TcpTunnelOptions tcpOptions )
                {
                    var message = new Message
                    {
                        Type = MessageType.Message,
                        Code = MessageCode.CreateTcpTunnelMessage
                    };
                    message.Values["port"] = tcpOptions.Port;

                    var response = await session.SendMessageAsync( message, token );

                    if ( !response.Success )
                    {
                        Console.WriteLine( $"Failed to initialize TCP tunnel for '{tcpOptions.Port}': {response.Message}" );
                        return false;
                    }

                    var tunnelId = response.Values["tunnel_id"].ToString().AsGuid();
                    var tunnelPort = response.Values["port"].ToString().AsInteger();

                    session.AddTunnel( new EchoTunnel( tunnelId ) );

                    Console.WriteLine( $"TCP Tunnel {tunnelPort} => {tcpOptions.TargetHostname}:{tcpOptions.TargetPort} ready." );
                }
                else if ( options is EchoTunnelOptions echoOptions )
                {
                    var message = new Message
                    {
                        Type = MessageType.Message,
                        Code = MessageCode.CreateTcpTunnelMessage
                    };
                    message.Values["port"] = echoOptions.Port;

                    var response = await session.SendMessageAsync( message, token );

                    if ( !response.Success )
                    {
                        Console.WriteLine( $"Failed to initialize TCP tunnel for '{echoOptions.Port}': {response.Message}" );
                        return false;
                    }

                    var tunnelId = response.Values["tunnel_id"].ToString().AsGuid();
                    var tunnelPort = response.Values["port"].ToString().AsInteger();

                    session.AddTunnel( new EchoTunnel( tunnelId ) );

                    Console.WriteLine( $"Echo Tunnel {tunnelPort}." );
                }
            }

            return true;
        }
    }
}
