using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace BlueBoxMoon.LocalSubway.Server.Authentication
{
    /// <summary>
    /// Provides authentication services for users.
    /// </summary>
    public interface IAuthenticationProvider
    {
        /// <summary>
        /// Authenticates the tunnel request.
        /// </summary>
        /// <param name="request">The request to be authenticated.</param>
        /// <param name="scheme">The scheme that should be provided to the principal.</param>
        /// <returns>A <see cref="AuthenticateResult"/> that indicates if the request was authenticated.</returns>
        Task<AuthenticateResult> AuthenticateTunnelRequestAsync( HttpRequest request, string scheme );
    }
}
