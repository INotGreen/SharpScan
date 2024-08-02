namespace CoreFtp.Components.DnsResolution
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Enum;
    using Infrastructure.Caching;

    public class DnsResolver : IDnsResolver
    {
        private readonly ICache cache;

        public DnsResolver()
        {
            cache = new InMemoryCache();
        }

        public async Task<IPEndPoint> ResolveAsync( string endpoint, int port, IpVersion ipVersion = IpVersion.IpV4, CancellationToken token = default( CancellationToken ) )
        {
            string cacheKey = $"{endpoint}:{port}:{ipVersion}";

            if ( port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort )
                throw new ArgumentOutOfRangeException( nameof( port ) );

            if ( cache.HasKey( cacheKey ) )
                return cache.Get<IPEndPoint>( cacheKey );

            var addressFamily = ipVersion.HasFlag( IpVersion.IpV4 )
                ? AddressFamily.InterNetwork
                : AddressFamily.InterNetworkV6;


            token.ThrowIfCancellationRequested();

            IPEndPoint ipEndpoint;

            var ipAddress = TryGetIpAddress( endpoint );

            if ( ipAddress != null )
            {
                ipEndpoint = new IPEndPoint( ipAddress, port );
                cache.Add( cacheKey, ipEndpoint, TimeSpan.FromMinutes( 60 ) );
                return ipEndpoint;
            }

            try
            {
                var allAddresses = await Dns.GetHostAddressesAsync( endpoint );

                var firstAddressInFamily = allAddresses.FirstOrDefault( x => x.AddressFamily == addressFamily );
                if ( firstAddressInFamily != null )
                {
                    ipEndpoint = new IPEndPoint( firstAddressInFamily, port );
                    cache.Add( cacheKey, ipEndpoint, TimeSpan.FromMinutes( 60 ) );
                    return ipEndpoint;
                }


                if ( addressFamily == AddressFamily.InterNetwork && ipVersion.HasFlag( IpVersion.IpV6 ) )
                {
                    ipEndpoint = await ResolveAsync( endpoint, port, IpVersion.IpV6, token );

                    if ( ipEndpoint != null )
                    {
                        cache.Add( cacheKey, ipEndpoint, TimeSpan.FromMinutes( 60 ) );
                        return ipEndpoint;
                    }
                }

                var firstAddress = allAddresses.FirstOrDefault();
                if ( firstAddress == null )
                    return null;

                switch ( firstAddress.AddressFamily )
                {
                    case AddressFamily.InterNetwork:
                        ipEndpoint = new IPEndPoint( firstAddress.MapToIPv6(), port );
                        break;

                    case AddressFamily.InterNetworkV6:
                        ipEndpoint = new IPEndPoint( firstAddress.MapToIPv4(), port );
                        break;
                    default:
                        return null;
                }

                cache.Add( cacheKey, ipEndpoint, TimeSpan.FromMinutes( 60 ) );

                return ipEndpoint;
            }
            catch
            {
                return null;
            }
        }

        private IPAddress TryGetIpAddress( string endpoint )
        {
            var tokens = endpoint.Split( ':' );

            string endpointToParse = endpoint;

            if ( tokens.Length == 0 )
                return null;

            if ( tokens.Length <= 2 )
            {
                // IPv4
                endpointToParse = tokens[ 0 ];
            }
            else if ( tokens.Length > 2 )
            {
                // IPv6
                endpointToParse = tokens[ 0 ].StartsWith( "[" ) && tokens[ tokens.Length - 2 ].EndsWith( "]" )
                    ? string.Join( ":", tokens.Take( tokens.Length - 1 ).ToArray() )
                    : endpoint;
            }

            IPAddress address;
            return IPAddress.TryParse( endpointToParse, out address )
                ? address
                : null;
        }
    }
}