using System.Threading;
using System.Threading.Tasks;

using BlueBoxMoon.LocalSubway.Http;

using Microsoft.AspNetCore.Http;

namespace BlueBoxMoon.LocalSubway.Server
{
    /// <summary>
    /// A Http Response Interceptor for use with WebConnection classes.
    /// </summary>
    /// <seealso cref="BlueBoxMoon.LocalSubway.HttpResponseInterceptor" />
    public class WebConnectionResponseInterceptor : HttpResponseInterceptor
    {
        #region Fields

        /// <summary>
        /// The response to be written to.
        /// </summary>
        private readonly HttpResponse _response;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WebConnectionResponseInterceptor"/> class.
        /// </summary>
        /// <param name="response">The response.</param>
        public WebConnectionResponseInterceptor( HttpResponse response )
            : base( response.Body )
        {
            _response = response;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Writes the headers to the output.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected override async Task WriteHeadersAsync( CancellationToken cancellationToken )
        {
            _response.StatusCode = StatusCode;

            foreach ( var header in Headers )
            {
                foreach ( var value in header.Value )
                {
                    _response.Headers.Append( header.Key, value );
                }
            }

            await _response.StartAsync();
        }

        #endregion
    }
}
