namespace CoreFtp.Infrastructure
{
    using System;

    public class FtpException : Exception
    {
        public FtpException() {}

        public FtpException( string message ) : base( message ) {}

        public FtpException( string message, Exception innerException ) : base( message, innerException ) {}
    }
}