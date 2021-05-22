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

    /// <summary>
    /// The main class that configures all the services and how the application
    /// will run.
    /// </summary>
    public class Startup
    {
        #region Properties

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public IConfiguration Configuration { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public Startup( IConfiguration configuration )
        {
            Configuration = configuration;
        }

        #endregion

        #region Methods

        /// <summary>
        /// This method gets called by the runtime. Use this method to add
        /// services to the container.
        /// </summary>
        /// <param name="services">The services.</param>
        public void ConfigureServices( IServiceCollection services )
        {
            var config = Configuration.Get<BasicConfiguration>();

            services.AddControllers();

            //
            // Register the authentication handler that will authenticate
            // all the incoming websocket requests.
            //
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, TunnelAuthenticationHandler>( AuthenticationSchemes.Tunnel, null );

            //
            // Register the singleton that will manage all requests to
            // different subdomains.
            //
            services.AddSingleton<SubwayDomainManager>();

            //
            // Register the authentication type that will be performed by
            // the authentication handler.
            //
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

        /// <summary>
        /// Configures how incoming HTTP requests will be processed.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The environment.</param>
        public void Configure( IApplicationBuilder app, IWebHostEnvironment env )
        {
            if ( env.IsDevelopment() )
            {
                app.UseDeveloperExceptionPage();
            }

            //
            // Enable and configure web sockets.
            //
            app.UseWebSockets( new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds( 60 )
            } );

            //app.UseHttpsRedirection();

            //
            // Map requests to /connect to the web socket middleware that
            // will handle incoming client connections. This will skip any
            // non websocket requests.
            //
            app.Map( "/connect", a => a.UseMiddleware<WebSocketManagerMiddleware>() );

            //
            // Fallthrough to the subway proxy that will handle requests to
            // registered subdomains.
            //
            app.UseMiddleware<SubwayProxyMiddleware>();

            //
            // If all else fails, return a not found page.
            //
            app.Run( async ( context ) =>
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync( "Not Found" );
            } );
        }

        #endregion
    }
}
