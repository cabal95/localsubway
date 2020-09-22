using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace BlueBoxMoon.LocalSubway.Server
{
    public class WebSocketManagerMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly SubwayDomainManager _domainManager;

        public WebSocketManagerMiddleware( RequestDelegate next, SubwayDomainManager domainManager )
        {
            _next = next;
            _domainManager = domainManager;
        }

        public async Task Invoke( HttpContext context )
        {
            if ( !context.WebSockets.IsWebSocketRequest )
            {
                await _next.Invoke( context );

                return;
            }

            var socket = await context.WebSockets.AcceptWebSocketAsync();

            var session = new SubwayServerSession( socket, _domainManager );

            try
            {
                await session.RunAsync( context.RequestAborted );
            }
            catch
            {
                await session.CloseAsync();
            }
        }
    }
}
