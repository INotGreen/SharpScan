using System;
using System.IO;
using System.Text;

namespace SharpRDPCheck
{
    internal sealed class MD4
    {
  
        public static byte[] ComputeHash(byte[] input)
        {
            MD4Managed managed = new MD4Managed();
            managed.Initialize();
            return managed.ComputeHash(input);
        }

        public static byte[] ComputeHash(string input, Encoding encoding)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input", "Unable to calculate hash over null input data");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding", "Unable to calculate hash over a string without a default encoding. Consider using the GetHash(string) overload to use UTF8 Encoding");
            }
            return ComputeHash(encoding.GetBytes(input));
        }

        public static string ComputeHashString(byte[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input", "Unable to calculate hash over null input data");
            }
            return BitConverter.ToString(ComputeHash(input)).Replace("-", "");
        }

    }
}