namespace Tamir.SharpSsh.Sharp.util
{
    /// <summary>
    /// Summary description for SharpString.
    /// </summary>
    public class SharpString : String
    {
        public SharpString(string s) : base(s)
        {
        }

        public SharpString(object o) : base(o)
        {
        }

        public SharpString(byte[] arr) : base(arr)
        {
        }

        public SharpString(byte[] arr, int offset, int len) : base(arr, offset, len)
        {
        }
    }
}