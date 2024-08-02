namespace CoreFtp.Infrastructure
{
    public static class Constants
    {
        public static readonly string ANONYMOUS_USER = "anonymous";
        public static readonly string UTF8 = "UTF8";
        public static readonly char LINEFEED = '\n';
        public static readonly string CARRIAGE_RETURN = "\r";
        public static readonly int BUFFER_SIZE = 512;

        public static readonly int FtpPort = 21;
        public static readonly int FtpsPort = 990;
    }
}