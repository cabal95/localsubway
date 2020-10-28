using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

using BlueBoxMoon.LocalSubway.Messages;
using BlueBoxMoon.LocalSubway.Server.Authentication;
using BlueBoxMoon.LocalSubway.Sessions;
using BlueBoxMoon.LocalSubway.Tunnels;

using Microsoft.AspNetCore.Http;

namespace BlueBoxMoon.LocalSubway.Server
{
    public class SubwayServerSession : ServerSession
    {
        private readonly SubwayDomainManager _domainManager;

        private readonly IAuthenticationProvider _authenticationProvider;

        private readonly HttpContext _context;

        public SubwayServerSession( WebSocket socket, HttpContext context, SubwayDomainManager domainManager, IAuthenticationProvider authenticationProvider )
            : base( socket, new JsonConverter() )
        {
            _domainManager = domainManager;
            _context = context;
            _authenticationProvider = authenticationProvider;
        }

        protected override Task RemoveTunnelAsync( Tunnel tunnel, bool notifyClient )
        {
            if ( tunnel is WebTunnel webTunnel )
            {
                _domainManager.RemoveTunnel( webTunnel );
            }

            return base.RemoveTunnelAsync( tunnel, notifyClient );
        }

        protected override Task<Response> CreateTcpTunnelAsync( Guid messageId, int port )
        {
            var tunnel = new TcpListenerTunnel( this, port );

            AddTunnel( tunnel );

            var response = new Response( messageId, true, $"Tunnel created on port {tunnel.Port}." );
            response.Values["tunnel_id"] = tunnel.Id;
            response.Values["port"] = tunnel.Port;

            return Task.FromResult( response );
        }

        protected override Task<Response> CreateWebTunnelAsync( Guid messageId, string domain )
        {
            var tunnel = new WebTunnel( this, domain );

            if ( _domainManager.AddTunnel( tunnel ) )
            {
                AddTunnel( tunnel );

                var url = $"https://{domain}.{_domainManager.Domain}";
                var response = new Response( messageId, true, $"Tunnel created at {url}" );
                response.Values["tunnel_id"] = tunnel.Id;
                response.Values["url"] = url;

                return Task.FromResult( response );
            }
            else
            {
                return Task.FromResult( new Response( messageId, false, $"Tunnel domain already in use." ) );
            }
        }
    }
}
