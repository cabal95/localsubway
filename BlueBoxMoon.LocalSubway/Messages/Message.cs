using System;
using System.Collections.Generic;

namespace BlueBoxMoon.LocalSubway.Messages
{
    /// <summary>
    /// Defines the structure of a message that is sent between client and server
    /// </summary>
    public class Message
    {
        #region Properties

        /// <summary>
        /// Gets or sets the message type.
        /// </summary>
        /// <value>
        /// The message type.
        /// </value>
        public MessageCode Code { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public MessageType Type { get; set; } = MessageType.Message;

        /// <summary>
        /// Gets or sets the message identifier.
        /// </summary>
        /// <value>
        /// The message identifier.
        /// </value>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the values of the message.
        /// </summary>
        /// <value>
        /// The values of the message.
        /// </value>
        public Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();

        #endregion
    }
}
