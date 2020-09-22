using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using BlueBoxMoon.LocalSubway.Client.Tunnels;
using BlueBoxMoon.LocalSubway.Messages;

namespace BlueBoxMoon.LocalSubway.Client
{
    class Program
    {
        static async Task Main( string[] args )
        {
            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += ( sender, e ) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            Console.WriteLine( "Hello World!" );

            try
            {
                await Run( cts.Token );
            }
            catch ( TaskCanceledException )
            {
                /* Intentionally ignored, the user requested the cancel. */
            }

            Console.WriteLine( "All done" );
        }

        private static async Task Run( CancellationToken token )
        {
            var ws = new ClientWebSocket();

            await ws.ConnectAsync( new Uri( "wss://localhost:5001/connect" ), token );

            var session = new SubwayClientSession( ws );

            var sessionTask = session.RunAsync( token );

            var message = new Message
            {
                Type = MessageType.Message,
                Code = MessageCode.CreateTcpTunnelMessage
            };
            message.Values["port"] = 8888;

            var response = await session.SendMessageAsync( message, token );

            Console.WriteLine( $"Tunnel: {response.Success} {response.Message}" );
            session.AddTunnel( new EchoTunnel( response.Values["tunnel_id"].ToString().AsGuid() ) );

            message = new Message
            {
                Type = MessageType.Message,
                Code = MessageCode.CreateWebTunnelMessage
            };
            message.Values["domain"] = "localhost";

            response = await session.SendMessageAsync( message, token );

            Console.WriteLine( $"Tunnel2: {response.Success} {response.Message}" );
            session.AddTunnel( new WebTunnel( response.Values["tunnel_id"].ToString().AsGuid(), "www.blueboxmoon.com", 443, true ) );

            await sessionTask;
        }
    }
}
