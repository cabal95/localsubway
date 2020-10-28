namespace BlueBoxMoon.LocalSubway.Server.Configuration
{
    /// <summary>
    /// The configuration that applies to the <see cref="Authentication.ApiKeyAuthenticationProvider"/>.
    /// </summary>
    public class ApiKeyAuthenticationConfiguration
    {
        /// <summary>
        /// Gets or sets the allowed tokens.
        /// </summary>
        /// <value>
        /// The allowed tokens.
        /// </value>
        public string AllowedTokens { get; set; }
    }
}
