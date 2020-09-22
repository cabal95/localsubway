using System;
using System.Threading.Tasks;

namespace BlueBoxMoon.LocalSubway
{
    /// <summary>
    /// Identifies a message that has been sent but no response has
    /// been received yet.
    /// </summary>
    internal sealed class PendingMessage
    {
        /// <summary>
        /// Gets the date the message was sent.
        /// </summary>
        /// <value>
        /// The date the message was sent.
        /// </value>
        public DateTimeOffset Date { get; } = DateTimeOffset.Now;

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public TaskCompletionSource<Messages.Response> Source { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PendingMessage"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public PendingMessage( TaskCompletionSource<Messages.Response> source )
        {
            Source = source;
        }
    }

}
