namespace BlueBoxMoon.LocalSubway.Server.Configuration
{
    /// <summary>
    /// The basic configuration that applies to the entire server.
    /// </summary>
    public class BasicConfiguration
    {
        /// <summary>
        /// Gets or sets the type of the authentication to use.
        /// </summary>
        /// <value>
        /// The type of the authentication to use.
        /// </value>
        public AuthenticationType AuthType { get; set; }
    }
}
