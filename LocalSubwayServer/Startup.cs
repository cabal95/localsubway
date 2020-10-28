using System;

using BlueBoxMoon.LocalSubway.Server.Authentication;
using BlueBoxMoon.LocalSubway.Server.Configuration;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
            var config = Configuration.Get<BasicConfiguration>();

            services.AddControllers();

            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, TunnelAuthenticationHandler>( AuthenticationSchemes.Tunnel, null );

            services.AddSingleton<SubwayDomainManager>();

            switch ( config.AuthType )
            {
                case AuthenticationType.None:
                    services.AddSingleton<IAuthenticationProvider, EmptyAuthenticationProvider>();
                    break;

                case AuthenticationType.ApiKey:
                    services.AddSingleton<IAuthenticationProvider, ApiKeyAuthenticationProvider>();
                    break;
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure( IApplicationBuilder app, IWebHostEnvironment env )
        {
            if ( env.IsDevelopment() )
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseWebSockets( new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds( 60 )
            } );

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
