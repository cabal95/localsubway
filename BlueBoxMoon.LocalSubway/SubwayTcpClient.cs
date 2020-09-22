using System;
using System.IO;
using System.Net.Sockets;

namespace BlueBoxMoon.LocalSubway
{
    public class SubwayTcpClient : IDisposable
    {
        private TcpClient _socket;

        public SubwayTcpClient( string hostname, int port )
        {
            _socket = new TcpClient( hostname, port );
        }

        public SubwayTcpClient( TcpClient socket )
        {
            _socket = socket;
        }

        public virtual Stream GetStream()
        {
            return _socket.GetStream();
        }

        protected virtual void Dispose( bool disposing )
        {
            if ( disposing )
            {
                if ( _socket != null )
                {
                    _socket.Dispose();
                    _socket = null;
                }
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose( disposing: true );
            GC.SuppressFinalize( this );
        }

        public virtual void Close()
        {
            _socket?.Close();
        }
    }
}
