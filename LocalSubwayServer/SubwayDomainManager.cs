using System.Collections.Concurrent;

namespace BlueBoxMoon.LocalSubway.Server
{
    public class SubwayDomainManager
    {
        private readonly ConcurrentDictionary<string, WebTunnel> _tunnels = new ConcurrentDictionary<string, WebTunnel>();

        public string Domain => "subwayapp.dev";

        public WebTunnel FindTunnel( string subdomain )
        {
            return _tunnels.TryGetValue( subdomain, out var tunnel ) ? tunnel : null;
        }

        public bool AddTunnel( WebTunnel tunnel )
        {
            return _tunnels.TryAdd( tunnel.Subdomain, tunnel );
        }

        public void RemoveTunnel( WebTunnel tunnel )
        {
            _tunnels.TryRemove( tunnel.Subdomain, out var _ );
        }
    }
}
