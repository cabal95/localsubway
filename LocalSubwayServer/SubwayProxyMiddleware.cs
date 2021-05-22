using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace BlueBoxMoon.LocalSubway.Server
{
    /// <summary>
    /// Handles the connections for the subway service and routes them to the
    /// appropriate client.
    /// </summary>
    public class SubwayProxyMiddleware
    {
        #region Fields

        /// <summary>
        /// The next request delegate to be called if we don't handle the
        /// request ourselves.
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// The domain manager.
        /// </summary>
        private readonly SubwayDomainManager _domainManager;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SubwayProxyMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next request handler.</param>
        /// <param name="domainManager">The domain manager.</param>
        public SubwayProxyMiddleware( RequestDelegate next, SubwayDomainManager domainManager )
        {
            _next = next;
            _domainManager = domainManager;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the request identified by the context.
        /// </summary>
        /// <param name="context">The context that covers the current request.</param>
        public async Task Invoke( HttpContext context )
        {
            var host = context.Request.Host.Host;
            var subdomain = host.Split( '.' )[0];

            var tunnel = _domainManager.FindTunnel( subdomain );

            //
            // If we couldn't find a tunnel for the subdomain then just chain
            // to the next handler.
            //
            if ( tunnel == null )
            {
                await _next.Invoke( context );
                return;
            }

            //
            // Initialize a new connection to handle this request.
            //
            var connection = new WebConnection( tunnel, context );

            try
            {
                //
                // Add the connection to the session tracker so it will be
                // processed later.
                //
                await tunnel.Session.AddConnection( connection, context.RequestAborted );
            }
            catch ( Exception ex )
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync( ex.Message );

                return;
            }

            //
            // Add a handler for the client aborting the request before we have
            // sent a response. When that happens remove the connection so it
            // will be disposed.
            //
            context.RequestAborted.Register( () =>
            {
                _ = tunnel.Session.RemoveConnectionAsync( connection );
            } );

            //
            // Wait for the connection to fully process.
            //
            await connection.ProcessRequestAsync();
        }

        #endregion
    }
}
