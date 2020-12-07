using System;
using System.IO;
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
        /// Reads and merge configuration files with the command line options.
        /// </summary>
        /// <param name="commandLineOptions">The command line options.</param>
        /// <returns>The final options to use.</returns>
        private static ProgramOptions ReadAndMergeConfigFiles( ProgramOptions commandLineOptions )
        {
            ProgramOptions options = new ProgramOptions();

            var homePath = Environment.GetFolderPath( Environment.SpecialFolder.UserProfile );
            var homeConfigPath = Path.Combine( homePath, ".localsubway" );

            if ( File.Exists( homeConfigPath ) )
            {
                Console.WriteLine( $"Reading configuration from {homeConfigPath}" );

                var json = File.ReadAllText( homeConfigPath );
                var homeOptions = System.Text.Json.JsonSerializer.Deserialize<ProgramOptions>( json );

                options.MergeOptionsFrom( homeOptions );
            }

            var currentConfigPath = Path.Combine( Environment.CurrentDirectory, ".localsubway" );
            if ( File.Exists( currentConfigPath ) )
            {
                Console.WriteLine( $"Reading configuration from {currentConfigPath}" );

                var json = File.ReadAllText( currentConfigPath );
                var currentPathOptions = System.Text.Json.JsonSerializer.Deserialize<ProgramOptions>( json );

                options.MergeOptionsFrom( currentPathOptions );
            }

            options.MergeOptionsFrom( commandLineOptions );

            return options;
        }

        /// <summary>
        /// Runs program with the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        private static async Task RunWithOptionsAsync( ProgramOptions options )
        {
            var userCancelled = false;

            options = ReadAndMergeConfigFiles( options );

            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += ( sender, e ) =>
            {
                e.Cancel = true;
                userCancelled = true;
                cts.Cancel();
            };

            do
            {
                try
                {
                    await RunAsync( options, cts.Token );
                }
                catch ( TaskCanceledException ) when ( userCancelled )
                {
                    /* Intentionally ignored, the user requested the cancel. */
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( $"Error communicating with server: {ex.Message}" );
                }
            } while ( options.Forever && !userCancelled );
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

            if ( options.Key != null && options.Key != string.Empty )
            {
                ws.Options.SetRequestHeader( "Authorization", $"Bearer {options.Key}" );
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

            await sessionTask;
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
