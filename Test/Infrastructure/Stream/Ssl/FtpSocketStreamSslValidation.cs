namespace CoreFtp.Infrastructure.Stream.Ssl
{
    public delegate void FtpSocketStreamSslValidation( FtpControlStream stream, FtpSslValidationEventArgs e );
}