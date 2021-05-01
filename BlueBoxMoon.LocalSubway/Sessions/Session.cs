using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using BlueBoxMoon.LocalSubway.Messages;

namespace BlueBoxMoon.LocalSubway.Sessions
{
    // TODO: probably should implement IDisposable
    public abstract class Session
    {
        #region Properties

        /// <summary>
        /// The messages that we are waiting for responses on.
        /// </summary>
        /// <value>
        /// The pending messages we are waiting for responses on.
        /// </value>
        private ConcurrentDictionary<Guid, PendingMessage> PendingMessages { get; } = new ConcurrentDictionary<Guid, PendingMessage>();

        /// <summary>
        /// The web socket we are connected to.
        /// </summary>
        /// <value>
        /// The socket.
        /// </value>
        private WebSocket Socket { get; }

        /// <summary>
        /// Gets the send queue.
        /// </summary>
        /// <value>
        /// The send queue.
        /// </value>
        private ConcurrentQueue<object> SendQueue { get; } = new ConcurrentQueue<object>();

        /// <summary>
        /// Gets the json converter.
        /// </summary>
        /// <value>
        /// The json converter.
        /// </value>
        protected IJsonConverter JsonConverter { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Session"/> class.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="jsonConverter">The json converter.</param>
        public Session( WebSocket socket, IJsonConverter jsonConverter )
        {
            Socket = socket;
            JsonConverter = jsonConverter;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Runs the processing loop for this session.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public virtual async Task RunAsync( CancellationToken cancellationToken )
        {
            await Task.WhenAll( RunSendAsync( cancellationToken ), RunReceiveAsync( cancellationToken ) );
        }

        /// <summary>
        /// Runs the receive loop for this session.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected virtual async Task RunReceiveAsync( CancellationToken cancellationToken )
        {
            var receiveStream = new MemoryStream();
            var buffer = new byte[4096];

            while ( Socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested )
            {
                var receiveResult = await Socket.ReceiveAsync( new ArraySegment<byte>( buffer ), cancellationToken );

                if ( receiveResult.MessageType == WebSocketMessageType.Close )
                {
                    break;
                }

                if ( receiveResult.MessageType == WebSocketMessageType.Text || receiveResult.MessageType == WebSocketMessageType.Binary )
                {
                    receiveStream.Write( buffer, 0, receiveResult.Count );

                    if ( !receiveResult.EndOfMessage )
                    {
                        continue;
                    }

                    receiveStream.Position = 0;

                    if ( receiveResult.MessageType == WebSocketMessageType.Text )
                    {
                        using ( var sr = new StreamReader( receiveStream, Encoding.UTF8 ) )
                        {
                            var json = sr.ReadToEnd();
                            var message = JsonConverter.DeserializeObject<Message>( json );

                            if ( message.Type == MessageType.Response )
                            {
                                ProcessResponse( Response.FromMessage( message ) );
                            }
                            else
                            {
                                await ProcessMessage( message );
                            }
                        }
                    }
                    else
                    {
                        var data = DataMessage.FromStream( receiveStream );

                        var bytes = data.Data;

                        if ( data.CompressionMode == DataCompressionMode.Deflate )
                        {
                            using ( var memoryStream = new MemoryStream() )
                            {
                                using ( var srcStream = new MemoryStream( bytes.Array, bytes.Offset, bytes.Count ) )
                                {
                                    using ( var deflateStream = new DeflateStream( srcStream, CompressionMode.Decompress, false ) )
                                    {
                                        deflateStream.CopyTo( memoryStream );
                                    }
                                }

                                bytes = new ArraySegment<byte>( memoryStream.ToArray() );
                            }
                        }

                        await ProcessData( data.ConnectionId, bytes );
                    }

                    receiveStream = new MemoryStream();
                }
            }
        }

        /// <summary>
        /// Runs the send loop for this session.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected virtual async Task RunSendAsync( CancellationToken cancellationToken )
        {
            while ( Socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested )
            {
                await Task.Delay( 20, cancellationToken );

                while ( SendQueue.TryDequeue( out var message ) )
                {
                    if ( message is DataMessage dataMessage )
                    {
                        var arrayBuffer = new ArraySegment<byte>( dataMessage.ToByteArray() );
                        await Socket.SendAsync( arrayBuffer, WebSocketMessageType.Binary, true, cancellationToken );
                    }
                    else if ( message is Message )
                    {
                        var json = JsonConverter.SerializeObject( message );
                        var arrayBuffer = new ArraySegment<byte>( Encoding.UTF8.GetBytes( json ) );
                        await Socket.SendAsync( arrayBuffer, WebSocketMessageType.Text, true, cancellationToken );
                    }
                }
            }
        }

        /// <summary>
        /// Processes the response.
        /// </summary>
        /// <param name="response">The response.</param>
        private void ProcessResponse( Response response )
        {
            if ( PendingMessages.TryRemove( response.Id, out var pendingMessage ) )
            {
                pendingMessage.Source.TrySetResult( response );
            }
            else
            {
                Console.WriteLine( $"Got unknown response to {response.Id}" );
            }
        }

        /// <summary>
        /// Sends the message as a notification without waiting for a response.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        public void SendNotification( Message message )
        {
            SendQueue.Enqueue( message );
        }

        /// <summary>
        /// Sends the message and waits for the response.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <returns>The response to the message.</returns>
        public async Task<Response> SendMessageAsync( Message message, CancellationToken cancellationToken, int timeout = 30000 )
        {
            var tcs = new TaskCompletionSource<Response>();

            using ( var cts = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken ) )
            {

                if ( timeout != Timeout.Infinite )
                {
                    cts.CancelAfter( timeout );
                }

                using ( cts.Token.Register( () => tcs.SetCanceled(), false ) )
                {
                    var pendingMessage = new PendingMessage( tcs );
                    PendingMessages.AddOrUpdate( message.Id, ( _ ) => pendingMessage, ( _1, _2 ) => pendingMessage );

                    SendNotification( message );

                    return await tcs.Task;
                }
            }
        }

        /// <summary>
        /// Sends the data to the remote endpoint.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="data">The data.</param>
        public Task SendDataAsync( Guid connectionId, ArraySegment<byte> data )
        {
#if false
            using ( var memoryStream = new MemoryStream() )
            {
                using ( var deflateStream = new DeflateStream( memoryStream, CompressionMode.Compress, false ) )
                {
                    deflateStream.Write( data.Array, data.Offset, data.Count );
                }

                data = new ArraySegment<byte>( memoryStream.ToArray() );
            }

            var message = new DataMessage
            {
                ConnectionId = connectionId,
                CompressionMode = DataCompressionMode.Deflate,
                Data = data
            };
#else
            var message = new DataMessage
            {
                ConnectionId = connectionId,
                CompressionMode = DataCompressionMode.None,
                Data = data
            };
#endif

            SendQueue.Enqueue( message );

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="response">The response.</param>
        protected void SendResponse( Response response )
        {
            SendNotification( response );
        }

        /// <summary>
        /// Processes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>A Task that indicates when processing has finished.</returns>
        protected abstract Task ProcessMessage( Message message );

        /// <summary>
        /// Processes the data message by passing the data through.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        protected virtual Task ProcessData( Guid connectionId, ArraySegment<byte> data )
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Closes this session.
        /// </summary>
        public virtual async Task CloseAsync()
        {
            try
            {
                await Socket.CloseAsync( WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None );
            }
            catch
            {
                /* Intentionally ignored since we might already be half closed. */
            }
        }

        #endregion
    }
}
