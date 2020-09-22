using System;
using System.IO;
using System.Text;

namespace BlueBoxMoon.LocalSubway.Messages
{
    public class DataMessage
    {
        /// <summary>
        /// Gets or sets the connection identifier.
        /// </summary>
        /// <value>
        /// The connection identifier.
        /// </value>
        public Guid ConnectionId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is compressed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is compressed; otherwise, <c>false</c>.
        /// </value>
        public bool IsCompressed { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public ArraySegment<byte> Data { get; set; }

        /// <summary>
        /// Froms the stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>The message read from the stream.</returns>
        /// <exception cref="InvalidDataException">Unsupported version number in data message.</exception>
        public static DataMessage FromStream( Stream stream )
        {
            using ( var reader = new BinaryReader( stream, Encoding.UTF8, true ) )
            {
                var message = new DataMessage();

                var version = reader.ReadByte();

                if ( version != 1 )
                {
                    throw new InvalidDataException( "Unsupported version number in data message." );
                }

                message.ConnectionId = new Guid( reader.ReadBytes( 16 ) );
                message.IsCompressed = reader.ReadBoolean();

                var len = reader.ReadUInt16();
                message.Data = new ArraySegment<byte>( reader.ReadBytes( len ) );

                return message;
            }
        }

        /// <summary>
        /// Converts to stream.
        /// </summary>
        /// <returns>A <see cref="Stream"/> that contains this encoded instance.</returns>
        public Stream ToStream()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter( stream, Encoding.UTF8, true );

            writer.Write( ( byte ) 1 );
            writer.Write( ConnectionId.ToByteArray() );
            writer.Write( false );
            writer.Write( ( ushort ) Data.Count );
            writer.Write( Data.Array, Data.Offset, Data.Count );

            stream.Position = 0;

            return stream;
        }

        /// <summary>
        /// Converts to bytearray.
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            var stream = ( MemoryStream ) ToStream();

            return stream.ToArray();
        }
    }
}
