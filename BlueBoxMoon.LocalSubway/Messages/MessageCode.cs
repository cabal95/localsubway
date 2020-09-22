namespace BlueBoxMoon.LocalSubway.Messages
{
    /// <summary>
    /// The various message codes supported.
    /// </summary>
    public enum MessageCode
    {
        /// <summary>
        /// Unknown message.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Create a new web tunnel.
        /// </summary>
        CreateWebTunnelMessage = 1000,

        /// <summary>
        /// Create a new TCP tunnel.
        /// </summary>
        CreateTcpTunnelMessage,

        /// <summary>
        /// Close a tunnel.
        /// </summary>
        CloseTunnelMessage,

        /// <summary>
        /// Close a connection.
        /// </summary>
        CloseConnectionMessage,

        /// <summary>
        /// Notification that a new connection has started.
        /// </summary>
        NewConnectionMessage,

        /// <summary>
        /// A connection has been closed.
        /// </summary>
        ConnectionClosedNotification = 2000,

        /// <summary>
        /// A tunnel has been closed, implies all the connections have also closed.
        /// </summary>
        TunnelClosedNotification,
    }
}
