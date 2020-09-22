namespace BlueBoxMoon.LocalSubway.Messages
{
    /// <summary>
    /// The type of message being sent or received.
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// A message that expects a response.
        /// </summary>
        Message,

        /// <summary>
        /// A response to a previous message.
        /// </summary>
        Response,

        /// <summary>
        /// A message that does not expect a response.
        /// </summary>
        Notification
    }
}
