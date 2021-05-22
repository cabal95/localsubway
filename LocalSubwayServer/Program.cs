using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BlueBoxMoon.LocalSubway.Server
{
    /// <summary>
    /// Main entry class for the server.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main( string[] args )
        {
            CreateHostBuilder( args ).Build().Run();
        }

        /// <summary>
        /// Creates the host builder that will build the service.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The host builder instance.</returns>
        public static IHostBuilder CreateHostBuilder( string[] args ) =>
            Host.CreateDefaultBuilder( args )
                .ConfigureWebHostDefaults( webBuilder =>
                 {
                     webBuilder.UseStartup<Startup>();
                 } );
    }
}
