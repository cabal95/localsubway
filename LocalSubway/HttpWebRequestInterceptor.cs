using System.Collections.Generic;
using System.IO;

using BlueBoxMoon.LocalSubway.Http;

namespace BlueBoxMoon.LocalSubway.Cli
{
    /// <summary>
    /// An interceptor for local web requests so we can rewrite headers.
    /// </summary>
    /// <seealso cref="BlueBoxMoon.LocalSubway.Http.HttpRequestInterceptor" />
    public class HttpWebRequestInterceptor : HttpRequestInterceptor
    {
        /// <summary>
        /// The forced headers to be set.
        /// </summary>
        public Dictionary<string, string> ForcedHeaders = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpWebRequestInterceptor"/> class.
        /// </summary>
        /// <param name="outputStream">The output stream.</param>
        public HttpWebRequestInterceptor( Stream outputStream )
            : base( outputStream )
        {
        }

        /// <summary>
        /// Prepares to write the headers and make any modifications required.
        /// </summary>
        protected override void PrepareToWriteHeaders()
        {
            base.PrepareToWriteHeaders();

            foreach ( var header in ForcedHeaders )
            {
                Headers[header.Key] = header.Value;
            }
        }
    }
}
