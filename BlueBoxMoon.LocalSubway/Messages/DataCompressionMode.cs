namespace BlueBoxMoon.LocalSubway.Messages
{
    /// <summary>
    /// The data compression mode of the <see cref="DataMessage"/>.
    /// </summary>
    public enum DataCompressionMode
    {
        /// <summary>
        /// Data is not compressed.
        /// </summary>
        None = 0,

        /// <summary>
        /// Data is compressed with the Deflate algorithm.
        /// </summary>
        Deflate = 1
    };
}
