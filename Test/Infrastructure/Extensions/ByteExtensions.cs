namespace CoreFtp.Infrastructure.Extensions
{
    using System.Text;

    public static class ByteExtensions
    {
        public static byte[] ToAsciiBytes( this string operand )
        {
            return Encoding.ASCII.GetBytes( $"{operand}\r\n".ToCharArray() );
        }
    }
}