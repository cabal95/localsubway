using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace BlueBoxMoon.LocalSubway.Server
{
    public class SubwayProxyMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly SubwayDomainManager _domainManager;

        public SubwayProxyMiddleware( RequestDelegate next, SubwayDomainManager domainManager )
        {
            _next = next;
            _domainManager = domainManager;
        }

        public async Task Invoke( HttpContext context )
        {
            var host = context.Request.Host.Host;
            var subdomain = host.Split( '.' )[0];

            var tunnel = _domainManager.FindTunnel( subdomain );

            if ( tunnel != null )
            {
                var connection = new WebConnection( tunnel, context );

                try
                {
                    await tunnel.Session.AddConnection( connection, context.RequestAborted );
                }
                catch ( Exception ex )
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync( ex.Message );

                    return;
                }

                context.RequestAborted.Register( () =>
                {
                    _ = tunnel.Session.RemoveConnectionAsync( connection );
                } );

                await connection.ProcessRequestAsync();
            }
            else
            {
                await _next.Invoke( context );
            }
        }
    }
}
