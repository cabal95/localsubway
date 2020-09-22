using System.IO;

namespace BlueBoxMoon.LocalSubway.Http
{
    /// <summary>
    /// An HttpInterceptor for responses.
    /// </summary>
    /// <seealso cref="BlueBoxMoon.LocalSubway.HttpInterceptor" />
    public class HttpResponseInterceptor : HttpInterceptor
    {
        #region Properties

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        /// <value>
        /// The status code.
        /// </value>
        public int StatusCode { get; protected set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseInterceptor"/> class.
        /// </summary>
        /// <param name="outputStream">The output stream.</param>
        public HttpResponseInterceptor( Stream outputStream )
            : base( outputStream )
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the first line to send in the request or response.
        /// </summary>
        /// <returns>
        /// The text of the first line, not including trailing end of line markers.
        /// </returns>
        protected override string GetFirstLine()
        {
            return $"{HttpVersion} {StatusCode} {Message}";
        }

        /// <summary>
        /// Parses the first line of the request or response.
        /// </summary>
        /// <param name="text">The line text.</param>
        /// <exception cref="InvalidDataException">Could not parse HTTP response line.</exception>
        protected override void ParseFirstLine( string text )
        {
            var segments = text.Split( new char[] { ' ' }, 3 );

            if ( segments.Length != 3 )
            {
                throw new InvalidDataException( "Could not parse HTTP response line." );
            }

            HttpVersion = segments[0];
            StatusCode = segments[1].AsInteger();
            Message = segments[2];
        }

        #endregion
    }
}
