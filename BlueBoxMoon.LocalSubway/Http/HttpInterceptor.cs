using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlueBoxMoon.LocalSubway.Http
{
    /// <summary>
    /// Intercepts stream writes and parses out any HTTP headers in a request
    /// or response data stream. Allows for modification before sending through
    /// to the final stream.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public abstract class HttpInterceptor : IDisposable
    {
        #region Fields

        /// <summary>
        /// The buffer stream used to temporarily store headers.
        /// </summary>
        private MemoryStream _bufferStream;

        /// <summary>
        /// <c>true</c> if <see cref="Dispose"/> has been called.
        /// </summary>
        private bool _isDisposed;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the output stream.
        /// </summary>
        /// <value>
        /// The output stream.
        /// </value>
        protected Stream OutputStream { get; private set; }

        /// <summary>
        /// Gets the headers.
        /// </summary>
        /// <value>
        /// The headers.
        /// </value>
        public Dictionary<string, string> Headers { get; private set; }

        /// <summary>
        /// Gets or sets the HTTP version.
        /// </summary>
        /// <value>
        /// The HTTP version.
        /// </value>
        public string HttpVersion { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpInterceptor"/> class.
        /// </summary>
        /// <param name="outputStream">The output stream.</param>
        public HttpInterceptor( Stream outputStream )
        {
            OutputStream = outputStream;
            _bufferStream = new MemoryStream();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose( bool disposing )
        {
            if ( !_isDisposed )
            {
                if ( disposing )
                {
                    OutputStream?.Dispose();
                    OutputStream = null;

                    _bufferStream?.Dispose();
                    _bufferStream = null;
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose( disposing: true );
            GC.SuppressFinalize( this );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Writes the content of the HTTP request or response.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset into the buffer.</param>
        /// <param name="count">The number of bytes to write.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task WriteAsync( byte[] buffer, int offset, int count, CancellationToken cancellationToken )
        {
            //
            // Check if we are direct streaming.
            //
            if ( _bufferStream == null )
            {
                await OutputStream.WriteAsync( buffer, offset, count, cancellationToken );

                return;
            }

            _bufferStream.Write( buffer, offset, count );
            var bufferBytes = _bufferStream.ToArray();
            var index = bufferBytes.IndexOf( HttpHeader.EndOfHeaders, 0, bufferBytes.Length );

            //
            // Check if we haven't received the end of header marker yet.
            //
            if ( index < 0 )
            {
                return;
            }

            //
            // Convert the header text into the individual headers.
            //
            var headerText = Encoding.UTF8.GetString( bufferBytes, 0, index );
            var lines = headerText.Split( new[] { "\r\n" }, StringSplitOptions.None );
            var firstLine = lines[0];
            Headers = lines.Skip( 1 )
                .Select( a => new HttpHeader( a ) )
                .ToList()
                .ToHeaderDictionary();

            ParseFirstLine( firstLine );

            //
            // Modify the data if needed.
            //
            PrepareToWriteHeaders();

            await WriteHeadersAsync( cancellationToken );

            //
            // Write any body data we may have already received.
            //
            await OutputStream.WriteAsync( bufferBytes, index + 4, bufferBytes.Length - index - 4, cancellationToken );

            //
            // Switch to direct streaming mode.
            //
            _bufferStream.Dispose();
            _bufferStream = null;
        }

        /// <summary>
        /// Parses the first line of the request or response.
        /// </summary>
        /// <param name="text">The line text.</param>
        protected abstract void ParseFirstLine( string text );

        /// <summary>
        /// Gets the first line to send in the request or response.
        /// </summary>
        /// <returns>The text of the first line, not including trailing end of line markers.</returns>
        protected abstract string GetFirstLine();

        /// <summary>
        /// Prepares to write the headers and make any modifications required.
        /// </summary>
        protected virtual void PrepareToWriteHeaders()
        {
        }

        /// <summary>
        /// Writes the headers to the output.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected virtual Task WriteHeadersAsync( CancellationToken cancellationToken )
        {
            var headerText = GetFirstLine() + "\r\n" + string.Join( "\r\n", Headers.Select( a => $"{a.Key}: {a.Value}" ) ) + "\r\n\r\n";
            var headerBytes = Encoding.UTF8.GetBytes( headerText );

            return OutputStream.WriteAsync( headerBytes, 0, headerBytes.Length, cancellationToken );
        }

        #endregion
    }
}
