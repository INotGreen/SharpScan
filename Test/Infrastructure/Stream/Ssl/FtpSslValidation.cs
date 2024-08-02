namespace CoreFtp.Infrastructure.Stream.Ssl
{
    using Stream;

    public delegate void FtpSslValidation( FtpControlStream control, FtpSslValidationEventArgs e );
}