namespace CoreFtp.Infrastructure.Extensions
{
    using Enum;

    public static class FtpStringExtensions
    {
        public static FtpStatusCode ToStatusCode( this string operand )
        {
            int statusCodeValue;
            int.TryParse( operand.Substring( 0, 3 ), out statusCodeValue );

            return statusCodeValue.ToNullableEnum<FtpStatusCode>() ?? FtpStatusCode.Undefined;
        }
    }
}