using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlueBoxMoon.LocalSubway.Server.Authentication
{
    /// <summary>
    /// Authentication handler for new Tunnel connection.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Authentication.AuthenticationHandler{BlueBoxMoon.LocalSubway.Server.Authentication.TunnelAuthenticationOptions}" />
    public class TunnelAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private IAuthenticationProvider _authenticationProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TunnelAuthenticationHandler"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="encoder">The encoder.</param>
        /// <param name="clock">The clock.</param>
        /// <param name="authenticationProvider">The authentication provider.</param>
        public TunnelAuthenticationHandler( IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IAuthenticationProvider authenticationProvider )
            : base( options, logger, encoder, clock )
        {
            _authenticationProvider = authenticationProvider;
        }

        /// <summary>
        /// Handles the authenticate asynchronous.
        /// </summary>
        /// <returns></returns>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            return _authenticationProvider.AuthenticateTunnelRequestAsync( Request, Scheme.Name );
        }
    }
}
