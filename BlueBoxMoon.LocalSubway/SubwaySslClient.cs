using System.IO;
using System.Net.Security;

namespace BlueBoxMoon.LocalSubway
{
    public class SubwaySslClient : SubwayTcpClient
    {
        private SslStream _sslStream;

        public SubwaySslClient( string hostname, int port )
            : base( hostname, port )
        {
            _sslStream = new SslStream( base.GetStream() );

            _sslStream.AuthenticateAsClient( hostname );
        }

        public override Stream GetStream()
        {
            return _sslStream;
        }

        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                if ( _sslStream != null )
                {
                    _sslStream.Dispose();
                    _sslStream = null;
                }
            }

            base.Dispose( disposing );
        }

        public override void Close()
        {
            _sslStream?.Close();

            base.Close();
        }
    }
}
