namespace BlueBoxMoon.LocalSubway.Server.Configuration
{
    /// <summary>
    /// The type of authentication provider to use.
    /// </summary>
    public enum AuthenticationType
    {
        /// <summary>
        /// No authentication is performed.
        /// </summary>
        None,

        /// <summary>
        /// The API key is authenticated against a list of valid keys.
        /// </summary>
        ApiKey,
    }
}
