using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using BlueBoxMoon.LocalSubway.Server.Authentication;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BlueBoxMoon.LocalSubway.Server
{

    public class Startup
    {
        public Startup( IConfiguration configuration )
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices( IServiceCollection services )
        {
            services.AddControllers();

            services.AddAuthentication()
                .AddScheme<TunnelAuthenticationOptions, TunnelAuthenticationHandler>( AuthenticationSchemes.Tunnel, null );

            services.AddSingleton<SubwayDomainManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure( IApplicationBuilder app, IWebHostEnvironment env )
        {
            if ( env.IsDevelopment() )
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseWebSockets();

            //app.UseHttpsRedirection();

            app.Map( "/connect", a => a.UseMiddleware<WebSocketManagerMiddleware>() );

            app.UseMiddleware<SubwayProxyMiddleware>();

            app.Run( async ( context ) =>
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync( "Not Found" );
            } );
        }
    }

}
