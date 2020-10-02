using System.Net.WebSockets;

using BlueBoxMoon.LocalSubway.Sessions;

namespace BlueBoxMoon.LocalSubway.Cli
{
    /// <summary>
    /// Handles session communication with the server.
    /// </summary>
    /// <seealso cref="BlueBoxMoon.LocalSubway.ClientSession" />
    public class SubwayClientSession : ClientSession
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SubwayClientSession"/> class.
        /// </summary>
        /// <param name="socket">The socket.</param>
        public SubwayClientSession( WebSocket socket )
            : base( socket, new JsonConverter() )
        {
        }

        #endregion
    }
}
