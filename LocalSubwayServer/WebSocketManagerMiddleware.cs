using System;
using System.Threading.Tasks;

using BlueBoxMoon.LocalSubway.Server.Authentication;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BlueBoxMoon.LocalSubway.Server
{
    /// <summary>
    /// Handles new incoming websocket connections from subway clients.
    /// </summary>
    public class WebSocketManagerMiddleware
    {
        #region Fields

        /// <summary>
        /// The next request handler to call if we don't handle the request.
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// The service provider used to create new objects.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketManagerMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next handler to call.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public WebSocketManagerMiddleware( RequestDelegate next, IServiceProvider serviceProvider )
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the incoming request or passes it on to the next handler.
        /// </summary>
        /// <param name="context">The context that identifies the request.</param>
        public async Task Invoke( HttpContext context )
        {
            //
            // If it's not a web socket request then we don't handle it.
            //
            if ( !context.WebSockets.IsWebSocketRequest )
            {
                await _next.Invoke( context );

                return;
            }

            //
            // Attempt to authenticate the client.
            //
            var authenticateResult = await context.AuthenticateAsync( AuthenticationSchemes.Tunnel );

            //
            // If they aren't authenticated, tell them to go away.
            //
            if ( !authenticateResult.Succeeded )
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync( "Unauthorized" );

                return;
            }

            context.User = authenticateResult.Principal;

            //
            // Upgrade the request to a websocket.
            //
            var socket = await context.WebSockets.AcceptWebSocketAsync();

            //
            // Create a new session to handle this user's connection.
            //
            var session = ActivatorUtilities.CreateInstance<SubwayServerSession>( _serviceProvider, socket );

            try
            {
                //
                // Run the session until the socket has closed.
                //
                await session.RunAsync( context.RequestAborted );
            }
            catch
            {
                await session.CloseAsync();
            }
        }

        #endregion
    }
}
