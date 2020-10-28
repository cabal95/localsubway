using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace BlueBoxMoon.LocalSubway.Server.Authentication
{
    /// <summary>
    /// An authentication provider that checks for a valid ApiKey against
    /// a set provided in the configuration.
    /// </summary>
    /// <seealso cref="BlueBoxMoon.LocalSubway.Server.Authentication.IAuthenticationProvider" />
    public class ApiKeyAuthenticationProvider : IAuthenticationProvider
    {
        #region Fields

        /// <summary>
        /// The authorization header name
        /// </summary>
        private const string AuthorizationHeaderName = "Authorization";

        /// <summary>
        /// The bearer scheme name
        /// </summary>
        private const string BearerSchemeName = "Bearer";

        /// <summary>
        /// The allowed tokens
        /// </summary>
        private readonly string[] _allowedTokens = new string[0];

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiKeyAuthenticationProvider"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public ApiKeyAuthenticationProvider( IConfiguration configuration )
        {
            var config = configuration.GetSection( "ApiKey" ).Get<Configuration.ApiKeyAuthenticationConfiguration>();

            if ( config.AllowedTokens != null && config.AllowedTokens != string.Empty )
            {
                _allowedTokens = config.AllowedTokens.Split( ',', StringSplitOptions.RemoveEmptyEntries );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Authenticates the tunnel request.
        /// </summary>
        /// <param name="request">The request to be authenticated.</param>
        /// <param name="scheme">The scheme that should be provided to the principal.</param>
        /// <returns>
        /// A <see cref="AuthenticateResult" /> that indicates if the request was authenticated.
        /// </returns>
        public Task<AuthenticateResult> AuthenticateTunnelRequestAsync( HttpRequest request, string scheme )
        {
            if ( !request.Headers.ContainsKey( AuthorizationHeaderName ) )
            {
                return Task.FromResult( AuthenticateResult.NoResult() );
            }

            if ( !AuthenticationHeaderValue.TryParse( request.Headers[AuthorizationHeaderName], out var headerValue ) )
            {
                return Task.FromResult( AuthenticateResult.NoResult() );
            }

            if ( !BearerSchemeName.Equals( headerValue.Scheme, StringComparison.OrdinalIgnoreCase ) )
            {
                return Task.FromResult( AuthenticateResult.NoResult() );
            }

            if ( _allowedTokens.Length == 0 )
            {
                return Task.FromResult( AuthenticateResult.NoResult() );
            }

            if ( !_allowedTokens.Contains( headerValue.Parameter ) )
            {
                return Task.FromResult( AuthenticateResult.Fail( "Invalid token." ) );
            }

            var claims = new List<Claim>
            {
                new Claim( "ApiKey", headerValue.Parameter ),
                new Claim( ClaimTypes.Name, headerValue.Parameter )
            };

            var identity = new ClaimsIdentity( claims, scheme );
            var principal = new ClaimsPrincipal( identity );
            var ticket = new AuthenticationTicket( principal, scheme );

            return Task.FromResult( AuthenticateResult.Success( ticket ) );
        }

        #endregion
    }
}
