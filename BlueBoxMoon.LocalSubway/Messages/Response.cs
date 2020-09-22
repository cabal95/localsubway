using System;

namespace BlueBoxMoon.LocalSubway.Messages
{
    /// <summary>
    /// A message that contains response information.
    /// </summary>
    /// <seealso cref="BlueBoxMoon.LocalSubway.Messages.Message" />
    public class Response : Message
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Response"/> is success.
        /// </summary>
        /// <value>
        ///   <c>true</c> if success; otherwise, <c>false</c>.
        /// </value>
        public bool Success
        {
            get => Values.ContainsKey( "success" ) && Values["success"].ToString().AsBoolean();
        }

        /// <summary>
        /// Gets or sets the message associated with this response.
        /// </summary>
        /// <value>
        /// The message associated with this response.
        /// </value>
        public string Message
        {
            get => Values.ContainsKey( "message" ) ? Values["message"].ToString() : null;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Response"/> class.
        /// </summary>
        public Response()
            : base()
        {
            Type = MessageType.Response;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Response" /> class.
        /// </summary>
        /// <param name="messageId">The message identifier to respond to.</param>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="message">The message that this response will respond to.</param>
        public Response( Guid messageId, bool success, string message )
            : this()
        {
            Type = MessageType.Response;
            Id = messageId;
            Values.Add( "success", success.ToString() );
            Values.Add( "message", message );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Generates a response object from the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        internal static Response FromMessage( Message message )
        {
            if ( message.Type != MessageType.Response )
            {
                throw new ArgumentOutOfRangeException( nameof( message ), "Message is not a response." );
            }

            return new Response
            {
                Code = message.Code,
                Type = message.Type,
                Id = message.Id,
                Values = message.Values
            };
        }

        #endregion
    }
}
