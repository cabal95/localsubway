using System;
using System.Threading.Tasks;

using BlueBoxMoon.LocalSubway.Server.Authentication;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BlueBoxMoon.LocalSubway.Server
{
    public class WebSocketManagerMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly IServiceProvider _serviceProvider;

        public WebSocketManagerMiddleware( RequestDelegate next, IServiceProvider serviceProvider )
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task Invoke( HttpContext context )
        {
            if ( !context.WebSockets.IsWebSocketRequest )
            {
                await _next.Invoke( context );

                return;
            }

            var authenticateResult = await context.AuthenticateAsync( AuthenticationSchemes.Tunnel );

            if ( !authenticateResult.Succeeded )
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync( "Unauthorized" );

                return;
            }

            context.User = authenticateResult.Principal;

            var socket = await context.WebSockets.AcceptWebSocketAsync();

            var session = ActivatorUtilities.CreateInstance<SubwayServerSession>( _serviceProvider, new object[] {
                socket,
                context
            } );

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
