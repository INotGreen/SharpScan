namespace CoreFtp.Components.DnsResolution
{
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Enum;

    public interface IDnsResolver
    {
        Task<IPEndPoint> ResolveAsync( string endpoint, int port, IpVersion ipVersion = IpVersion.IpV4, CancellationToken token = default( CancellationToken ) );
    }
}