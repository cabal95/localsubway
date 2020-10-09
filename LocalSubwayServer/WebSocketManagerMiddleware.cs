using System.Threading.Tasks;

using BlueBoxMoon.LocalSubway.Server.Authentication;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace BlueBoxMoon.LocalSubway.Server
{
    public class WebSocketManagerMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly SubwayDomainManager _domainManager;

        private readonly bool _requireAuthentication;

        public WebSocketManagerMiddleware( RequestDelegate next, SubwayDomainManager domainManager, IConfiguration configuration )
        {
            _next = next;
            _domainManager = domainManager;

            var allowedTokens = configuration["AllowedTokens"];

            if ( allowedTokens != null && allowedTokens != string.Empty )
            {
                _requireAuthentication = true;
            }
        }

        public async Task Invoke( HttpContext context )
        {
            if ( !context.WebSockets.IsWebSocketRequest )
            {
                await _next.Invoke( context );

                return;
            }

            if ( _requireAuthentication )
            {
                var authenticateResult = await context.AuthenticateAsync( AuthenticationSchemes.Tunnel );

                if ( !authenticateResult.Succeeded )
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync( "Unauthorized" );

                    return;
                }

                context.User = authenticateResult.Principal;
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
