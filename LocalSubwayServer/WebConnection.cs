﻿using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using BlueBoxMoon.LocalSubway.Connections;
using BlueBoxMoon.LocalSubway.Http;

using Microsoft.AspNetCore.Http;

namespace BlueBoxMoon.LocalSubway.Server
{
    /// <summary>
    /// Handles web connections being tunneled over the subway.
    /// </summary>
    /// <seealso cref="BlueBoxMoon.LocalSubway.Connection" />
    /// <remarks>
    /// TODO: There might be a glitch with keep-alive connections, need to test.</remarks>
    public class WebConnection : Connection
    {
        #region Fields

        /// <summary>
        /// The tunnel this connection is associated with.
        /// </summary>
        private readonly WebTunnel _tunnel;

        /// <summary>
        /// The context for this connection.
        /// </summary>
        private readonly HttpContext _context;

        /// <summary>
        /// The cancellation token source
        /// </summary>
        private readonly CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// The interceptor
        /// </summary>
        private readonly HttpResponseInterceptor _interceptor;

        #endregion

        #region Consctructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WebConnection"/> class.
        /// </summary>
        /// <param name="tunnel">The tunnel this connection is associated with.</param>
        /// <param name="context">The context.</param>
        public WebConnection( WebTunnel tunnel, HttpContext context )
            : base( tunnel.Id )
        {
            _tunnel = tunnel;
            _context = context;
            _interceptor = new WebConnectionResponseInterceptor( context.Response );

            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( context.RequestAborted );

        }

        #endregion

        #region Methods

        /// <summary>
        /// Processes the request, does not return until the connection has closed.
        /// </summary>
        public async Task ProcessRequestAsync()
        {
            await SendRequestAsync( _context.Request );

            while ( !_cancellationTokenSource.IsCancellationRequested )
            {
                try
                {
                    await Task.Delay( 1000, _cancellationTokenSource.Token );
                }
                catch ( TaskCanceledException )
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Sends the data to local side of the connection.
        /// </summary>
        /// <param name="data">The data to be sent.</param>
        public override async Task SendDataToLocalAsync( ArraySegment<byte> data )
        {
            Console.WriteLine( $"Writing {data.Count} bytes." );
            await _interceptor.WriteAsync( data.Array, data.Offset, data.Count, _cancellationTokenSource.Token );
            Console.WriteLine( "Write complete" );
        }

        /// <summary>
        /// Closes the local side of the connection.
        /// </summary>
        public override Task CloseLocalAsync()
        {
            _cancellationTokenSource.Cancel();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sends the request on to the remote session.
        /// </summary>
        /// <param name="request">The request.</param>
        private async Task SendRequestAsync( HttpRequest request )
        {
            var sb = new StringBuilder();

            ModifyHeaders( request);

            //
            // Rebuild the request.
            //
            sb.AppendLine( $"{request.Method} {request.Path}{request.QueryString} {request.Protocol}" );

            foreach ( var header in request.Headers )
            {
                sb.AppendLine( $"{header.Key}: {header.Value}" );
            }

            sb.AppendLine();

            //
            // Convert the header text into buffer data.
            //
            using var requestStream = new MemoryStream();
            var headerBytes = Encoding.UTF8.GetBytes( sb.ToString() );
            requestStream.Write( headerBytes, 0, headerBytes.Length );

            //
            // Append any body data we might have received.
            //
            if ( request.ContentLength.HasValue )
            {
                await request.Body.CopyToAsync( requestStream );
            }

            //
            // Send the request over the tunnel.
            //
            await _tunnel.Session.SendDataAsync( Id, new ArraySegment<byte>( requestStream.ToArray() ) );

        }

        /// <summary>
        /// Modifies the headers.
        /// </summary>
        /// <param name="request">The request.</param>
        protected virtual void ModifyHeaders( HttpRequest request )
        {
            if ( !request.Headers.ContainsKey( "X-Forwarded-For" ) )
            {
                request.Headers.Add( "X-Forwarded-For", _context.Connection.RemoteIpAddress.ToString() );
            }

            if ( !request.Headers.ContainsKey( "X-Forwarded-Proto" ) )
            {
                request.Headers.Add( "X-Forwarded-Proto", request.Scheme );
            }

            if ( !request.Headers.ContainsKey( "X-Forwarded-Port" ) )
            {
                request.Headers.Add( "X-Forwarded-Port", _context.Connection.LocalPort.ToString() );
            }

            if ( !request.Headers.ContainsKey( "X-Forwarded-Host" ) && request.Headers.ContainsKey( "Host" ) )
            {
                request.Headers.Add( "X-Forwarded-Host", request.Host.Host );
            }

        }

        #endregion
    }
}
