using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace BlueBoxMoon.LocalSubway.Server.Authentication
{
    /// <summary>
    /// An empty authentication provider that always allows access.
    /// </summary>
    /// <seealso cref="BlueBoxMoon.LocalSubway.Server.Authentication.IAuthenticationProvider" />
    public class EmptyAuthenticationProvider : IAuthenticationProvider
    {
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
            var identity = new ClaimsIdentity( new Claim[0], scheme );
            var principal = new ClaimsPrincipal( identity );
            var ticket = new AuthenticationTicket( principal, scheme );

            return Task.FromResult( AuthenticateResult.Success( ticket ) );
        }
    }
}
