using System;

namespace BlueBoxMoon.LocalSubway.Http
{
    /// <summary>
    /// Identifies a single header from the HTTP stream.
    /// </summary>
    public class HttpHeader
    {
        #region Properties

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHeader"/> class.
        /// </summary>
        /// <param name="headerLine">The header line.</param>
        public HttpHeader( string headerLine )
        {
            var pairs = headerLine.Split( new[] { ": " }, StringSplitOptions.None );
            Name = pairs[0];
            Value = pairs[1];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHeader"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public HttpHeader( string name, string value )
        {
            Name = name;
            Value = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{Name}: {Value}";
        }

        #endregion
    }
}
