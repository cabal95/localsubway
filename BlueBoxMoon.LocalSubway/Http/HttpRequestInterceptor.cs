using System.IO;

namespace BlueBoxMoon.LocalSubway.Http
{
    /// <summary>
    /// An HttpInterceptor for requests.
    /// </summary>
    /// <seealso cref="BlueBoxMoon.LocalSubway.HttpInterceptor" />
    public class HttpRequestInterceptor : HttpInterceptor
    {
        #region Properties

        /// <summary>
        /// Gets or sets the method.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        public string Method { get; protected set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestInterceptor"/> class.
        /// </summary>
        /// <param name="outputStream">The output stream.</param>
        public HttpRequestInterceptor( Stream outputStream )
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
            return $"{Method} {Path} {HttpVersion}";
        }

        /// <summary>
        /// Parses the first line of the request or response.
        /// </summary>
        /// <param name="text">The line text.</param>
        /// <exception cref="InvalidDataException">Could not parse HTTP request line.</exception>
        protected override void ParseFirstLine( string text )
        {
            var segments = text.Split( ' ' );

            if ( segments.Length != 3 )
            {
                throw new InvalidDataException( "Could not parse HTTP request line." );
            }

            Method = segments[0];
            Path = segments[1];
            HttpVersion = segments[2];
        }

        #endregion
    }
}
