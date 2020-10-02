using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using BlueBoxMoon.LocalSubway.Cli.Options;
using BlueBoxMoon.LocalSubway.Cli.Tunnels;
using BlueBoxMoon.LocalSubway.Messages;

using CommandLine;

namespace BlueBoxMoon.LocalSubway.Cli
{
    /// <summary>
    /// Main application processing loop.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static async Task Main( string[] args )
        {
            await Parser.Default
                .ParseArguments<ProgramOptions>( args )
                .WithParsedAsync( RunWithOptionsAsync );
        }

        /// <summary>
        /// Runs program with the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        private static async Task RunWithOptionsAsync( ProgramOptions options )
        {
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
        /// Runs the session.
        /// </summary>
        /// <param name="tunnelOptions">The tunnel options.</param>
        /// <param name="token">The cancellation token.</param>
        private static async Task RunAsync( ProgramOptions options, CancellationToken token )
        {
            var ws = new ClientWebSocket();
            string url = "wss://subwayapp.dev/";

            if ( options.Server != null )
            {
                url = options.Server.ToString().Replace( "http://", "ws://" ).Replace( "https://", "wss://" );
            }

            try
            {
                await ws.ConnectAsync( new Uri( $"{url}connect" ), token );
            }
            catch ( Exception ex )
            {
                Console.WriteLine( $"Unable to connect to server: {ex.Message}" );
                return;
            }

            var session = new SubwayClientSession( ws );

            var sessionTask = session.RunAsync( token );

            if ( !await SetupTunnelsAsync( session, options, token ) )
            {
                return;
            }

            try
            {
                await sessionTask;
            }
            catch ( Exception ex )
            {
                Console.WriteLine( $"Error communicating with server: {ex.Message}" );
            }
        }

        /// <summary>
        /// Setups the tunnels.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="tunnelOptions">The tunnel options.</param>
        /// <param name="token">The token.</param>
        /// <returns><c>true</c> if the tunnels were correctly setup.</returns>
        private static async Task<bool> SetupTunnelsAsync( SubwayClientSession session, ProgramOptions options, CancellationToken token )
        {
            if ( options.EchoTunnels.Count() == 0 && options.TcpTunnels.Count() == 0 && options.HttpTunnels.Count() == 0 && options.HttpsTunnels.Count() == 0 )
            {
                Console.WriteLine( "No tunnels were defined." );
                return false;
            }

            foreach ( var webOptions in options.HttpTunnels.Union( options.HttpsTunnels ) )
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

            foreach ( var tcpOptions in options.TcpTunnels )
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

            foreach ( var echoOptions in options.EchoTunnels )
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

            return true;
        }
    }
}
