using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlueBoxMoon.LocalSubway.Server.Authentication
{
    /// <summary>
    /// Authentication handler for new Tunnel connection.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Authentication.AuthenticationHandler{BlueBoxMoon.LocalSubway.Server.Authentication.TunnelAuthenticationOptions}" />
    public class TunnelAuthenticationHandler : AuthenticationHandler<TunnelAuthenticationOptions>
    {
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

        /// <summary>
        /// Initializes a new instance of the <see cref="TunnelAuthenticationHandler"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="encoder">The encoder.</param>
        /// <param name="clock">The clock.</param>
        /// <param name="configuration">The configuration.</param>
        public TunnelAuthenticationHandler( IOptionsMonitor<TunnelAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IConfiguration configuration )
            : base( options, logger, encoder, clock )
        {
            var allowedTokens = configuration["AllowedTokens"];

            if ( allowedTokens != null && allowedTokens != string.Empty )
            {
                _allowedTokens = allowedTokens.Split( ',', StringSplitOptions.RemoveEmptyEntries );
            }
        }

        /// <summary>
        /// Handles the authenticate asynchronous.
        /// </summary>
        /// <returns></returns>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if ( _allowedTokens.Length == 0 )
            {
                return Task.FromResult( AuthenticateResult.NoResult() );
            }

            if ( !Request.Headers.ContainsKey( AuthorizationHeaderName ) )
            {
                return Task.FromResult( AuthenticateResult.NoResult() );
            }

            if ( !AuthenticationHeaderValue.TryParse( Request.Headers[AuthorizationHeaderName], out var headerValue ) )
            {
                return Task.FromResult( AuthenticateResult.NoResult() );
            }

            if ( !BearerSchemeName.Equals( headerValue.Scheme, StringComparison.OrdinalIgnoreCase ) )
            {
                return Task.FromResult( AuthenticateResult.NoResult() );
            }

            if ( !_allowedTokens.Contains( headerValue.Parameter ) )
            {
                return Task.FromResult( AuthenticateResult.Fail( "Invalid token." ) );
            }

            var claims = new[] { new Claim( "ApiKey", headerValue.Parameter ) };
            var identity = new ClaimsIdentity( claims, Scheme.Name );
            var principal = new ClaimsPrincipal( identity );
            var ticket = new AuthenticationTicket( principal, Scheme.Name );

            return Task.FromResult( AuthenticateResult.Success( ticket ) );
        }
    }

}
