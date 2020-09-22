using BlueBoxMoon.LocalSubway.Sessions;
using BlueBoxMoon.LocalSubway.Tunnels;

namespace BlueBoxMoon.LocalSubway.Server
{
    public class WebTunnel : Tunnel
    {
        public string Subdomain { get; }

        public ServerSession Session { get; }

        public WebTunnel( ServerSession session, string subdomain )
        {
            Session = session;
            Subdomain = subdomain;
        }
    }
}
