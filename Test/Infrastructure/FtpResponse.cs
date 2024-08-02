namespace CoreFtp.Infrastructure
{
    using Enum;

    public class FtpResponse
    {
        public FtpStatusCode FtpStatusCode { get; set; }

        public bool IsSuccess
        {
            get
            {
                int statusCode = (int) FtpStatusCode;
                return statusCode >= 100 && statusCode < 400;
            }
        }

        public string ResponseMessage { get; set; }
        public string[] Data { get; set; }
        public string Request { get; set; }

        public static FtpResponse EmptyResponse = new FtpResponse
        {
            ResponseMessage = "No response was received",
            FtpStatusCode = FtpStatusCode.Undefined
        };
    }
}