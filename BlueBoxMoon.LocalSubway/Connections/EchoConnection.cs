using System;
using System.Threading.Tasks;

using BlueBoxMoon.LocalSubway.Sessions;

namespace BlueBoxMoon.LocalSubway.Connections
{
    /// <summary>
    /// A simple connection that echoes the data back to the sender.
    /// </summary>
    /// <seealso cref="BlueBoxMoon.LocalSubway.Connection" />
    public class EchoConnection : Connection
    {
        #region Fields

        /// <summary>
        /// The session
        /// </summary>
        private readonly Session _session;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EchoConnection"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="tunnelId">The tunnel identifier.</param>
        public EchoConnection( Session session, Guid id, Guid tunnelId )
            : base( id, tunnelId )
        {
            _session = session;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sends the data to local side of the connection.
        /// </summary>
        /// <param name="data">The data to be sent.</param>
        public override Task SendDataToLocalAsync( ArraySegment<byte> data )
        {
            return _session.SendDataAsync( Id, data );
        }

        #endregion
    }
}
