namespace CoreFtp
{
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using Enum;
    using Infrastructure;

    public class FtpClientConfiguration
    {
        public int TimeoutSeconds { get; set; } = 120;
        public int? DisconnectTimeoutMilliseconds { get; set; } = 100;
        public int Port { get; set; } = Constants.FtpPort;
        public string Host { get; set; }
        public IpVersion IpVersion { get; set; } = IpVersion.IpV4;
        public FtpEncryption EncryptionType { get; set; } = FtpEncryption.None;
        public bool IgnoreCertificateErrors { get; set; } = true;
        public string Username { get; set; }
        public string Password { get; set; }
        public string BaseDirectory { get; set; } = "/";
        public FtpTransferMode Mode { get; set; } = FtpTransferMode.Binary;
        public char ModeSecondType { get; set; } = '\0';

        public bool ShouldEncrypt => EncryptionType == FtpEncryption.Explicit ||
                                     EncryptionType == FtpEncryption.Implicit &&
                                     Port == Constants.FtpsPort;

        public X509CertificateCollection ClientCertificates { get; set; } = new X509CertificateCollection();
        public SslProtocols SslProtocols { get; set; } = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
    }
}
