namespace CoreFtp.Infrastructure
{
    using Enum;
    using Extensions;

    public class FtpCommandEnvelope
    {
        public FtpCommand FtpCommand { get; set; }
        public string Data { get; set; }

        public string GetCommandString()
        {
            string command = FtpCommand.ToString();

            return Data.IsNullOrEmpty()
                ? command
                : $"{command} {Data}";
        }
    }
}