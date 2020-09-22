using System;
using System.Threading.Tasks;

namespace BlueBoxMoon.LocalSubway.Connections
{
    /// <summary>
    /// Identifies a single connection inside a tunnel between the client
    /// and the server.
    /// </summary>
    public class Connection
    {
        #region Properties

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public Guid Id { get; }

        /// <summary>
        /// Gets the tunnel identifier.
        /// </summary>
        /// <value>
        /// The tunnel identifier.
        /// </value>
        public Guid TunnelId { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Connection"/> class.
        /// </summary>
        /// <param name="tunnelId">The tunnel identifier.</param>
        public Connection( Guid tunnelId )
        {
            Id = Guid.NewGuid();
            TunnelId = tunnelId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Connection"/> class.
        /// </summary>
        /// <param name="id">The connection identifier.</param>
        /// <param name="tunnelId">The tunnel identifier.</param>
        public Connection( Guid id, Guid tunnelId )
        {
            Id = id;
            TunnelId = tunnelId;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sends the data to local side of the connection.
        /// </summary>
        /// <param name="data">The data to be sent.</param>
        /// <returns></returns>
        public virtual Task SendDataToLocalAsync( ArraySegment<byte> data )
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Closes the local side of the connection.
        /// </summary>
        /// <returns></returns>
        public virtual Task CloseLocalAsync()
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}
