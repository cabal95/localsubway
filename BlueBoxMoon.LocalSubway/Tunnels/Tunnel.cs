using System;
using System.Threading.Tasks;

namespace BlueBoxMoon.LocalSubway.Tunnels
{
    /// <summary>
    /// Identifies a tunnel between the client and server.
    /// </summary>
    public class Tunnel
    {
        #region Properties

        /// <summary>
        /// Gets the identifier of the tunnel.
        /// </summary>
        /// <value>
        /// The identifier of the tunnel.
        /// </value>
        public Guid Id { get; } = Guid.NewGuid();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Tunnel"/> class.
        /// </summary>
        public Tunnel()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tunnel"/> class.
        /// </summary>
        /// <param name="id">The tunnel identifier.</param>
        public Tunnel( Guid id )
        {
            Id = id;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Closes the local side of the tunnel, for example a server might
        /// close a listening TCP port.
        /// </summary>
        /// <returns></returns>
        public virtual Task CloseLocalAsync()
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}
